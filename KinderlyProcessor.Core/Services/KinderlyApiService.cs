using KinderlyProcessor.Core.Interfaces;
using KinderlyProcessor.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace KinderlyProcessor.Core.Services
{
    public class KinderlyApiService : IKinderlyApiService
    {
        private readonly IBluelightApiService _bluelightApiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEmailSercive _emailService;
        private readonly ILogger<KinderlyApiService> _logger;
        private readonly Dictionary<string, string> _applicationSetting;

        public KinderlyApiService(IBluelightApiService bluelightApiService, IHttpClientFactory httpClientFactory, IEmailSercive emailService, ILogger<KinderlyApiService> logger, IOptions<Dictionary<string, string>> options)
        {
            _bluelightApiService = bluelightApiService;
            _httpClientFactory = httpClientFactory;
            _emailService = emailService;
            _logger = logger;
            _applicationSetting = options.Value;
        }

        /// <summary>
        /// Appoves Pacey members for Kinderly product 
        /// </summary>
        /// <returns>A list of approved Pacey members </returns>
        public async Task<List<KinderlyMembership>> ApprovePaceyMembers()
        {
            var client = _httpClientFactory.CreateClient("KinderlyApi");
            var kinderlyMemberships = new List<KinderlyMembership>();

            //Retrive a list of unapproved Pacey members from Bluelight 
            var members = await _bluelightApiService.GetMembersAsync();

            if (members.Count == 0) return kinderlyMemberships;

            // Creates a list of Pacey members with data from Bluelight using Json Model
            var paceyMembers = members.Select(member => new PaceyMembership()
            {
                ContactId = member.Contact.Id,
                Email = member.Contact.EmailAddress1,
                Postcode = member.Contact.Address2Postcode,
                FirstName = member.Contact.FirstName,
                LastName = member.Contact.LastName,
                Membership = new Membership
                {
                    MembershipSubType = member.SubType.Label,
                    MembershipClass = member.Class.Name,
                    MembershipNo = member.MembershipNumber,
                    MembershipGrade = member.Grade.Name,
                    MembershipId = member.Id,
                    PaymentMethod = member.PaymentMethod.Label,
                    MembershipValidFrom = member.ValidFrom.ToString(CultureInfo.InvariantCulture),
                    MembershipValidTo = member.ValidTo.ToString(CultureInfo.InvariantCulture)
                }
            }).ToList();

            // Send Pacey memebers to Kinderly 
            var response = await client.PostAsJsonAsync(client.BaseAddress, new { data = paceyMembers });

            // Once approved, Kinderly returns the same list of members but with addtional information 
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(result);

            // Use Kinderly response date to create a list of now Kinderly members using Json model 
            dynamic kinderlyResponse = JsonConvert.DeserializeObject(jsonResponse);

            var unApprovedMembers = new List<dynamic>();

            if (kinderlyResponse == null)
                return kinderlyMemberships;

            foreach (var member in kinderlyResponse.data)
            {
                string membershipId;
                string email;
                if (member.success == true)
                {
                    membershipId = member.customer.membership_id;
                    email = member.customer.email;

                    var kinderlyMember = new KinderlyMembership
                    {
                        Id = member.customer.membership_id,

                        Contact = new Contact
                        {
                            Id = member.customer.contact_id
                        },

                        KinderlyLead = member.customer.kinderly_lead == "1",
                        KinderlyMember = member.customer.kt_member == "1"
                    };

                    _logger.LogInformation("Kinderly member with Membership Id: {0} and Email: {1} processed succesfully.", membershipId, email);
                    _logger.LogInformation(result);
                    kinderlyMemberships.Add(kinderlyMember);
                }
                else
                {
                    membershipId = member.customer.membership.membership_id;
                    email = member.customer.email;
                    _logger.LogError("Kinderly member with Membership Id: {0} and Email: {1} processed unsuccesfully.", membershipId, email);
                    _logger.LogError(result);
                    unApprovedMembers.Add(member);
                    await _emailService.SendUnapprovedMembers(unApprovedMembers);
                }
            }
            return kinderlyMemberships;
        }

        public async Task<List<DigitalContract>> GetDigitalContractsAsync()
        {
            //Retrive a list of digital contracts from Bluelight 
            var contracts = await _bluelightApiService.GetContracts();

            var digitalContracts = new List<DigitalContract>();
            var orders = new List<OrderSummary>();
            var unrecognisedProducts = new List<dynamic>();

            if (contracts.Count == 0)
                return digitalContracts;

            foreach (var contract in contracts)
            {
                orders.AddRange(contract.Products.Select(item => new OrderSummary { Quantity = item.Quantity, UrId = item.Product.Name, OrderRefId = contract.CmsId }));

                var digitalContract = new DigitalContract
                {
                    Invoice = contract.Products.Select(item => item.Invoice.Id).FirstOrDefault(),
                    Email = contract.Contact.EmailAddress1,
                    FirstName = contract.Contact.FirstName,
                    LastName = contract.Contact.LastName,
                    OrderSummary = orders
                };

                digitalContracts.Add(digitalContract);
            }

            foreach (var contract in digitalContracts)
            {
                foreach (var product in contract.OrderSummary)
                {
                    if (_applicationSetting.ContainsKey(product.UrId))
                    {
                        product.UrId = _applicationSetting[product.UrId];
                    }
                    else
                    {
                        unrecognisedProducts.Add(product.UrId);
                    }
                }

                if (!unrecognisedProducts.Any())
                    continue;

                var item = new
                {
                    email = contract.Email,
                    first_name = contract.FirstName,
                    last_name = contract.LastName,
                    invoice = contract.Invoice,
                    products = unrecognisedProducts
                };

                await _emailService.SendUnrecognisedProducts(item);
            }

            return digitalContracts;
        }

        public bool IsValid(string emailaddress)
        {
            try
            {
                _ = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends Pacey Members approved Kinderly back to Bluelight  
        /// </summary>
        public async Task ProcessDigitalContractsAsync(string api)
        {
            var client = _httpClientFactory.CreateClient(api);
            var digitalContracts = await GetDigitalContractsAsync();
            var failedContracts = new List<dynamic>();

            if (!digitalContracts.Any())
                return;

            foreach (var contract in digitalContracts)
            {
                var email = contract.Email;
                var data = new
                {
                    email,
                    first_name = contract.FirstName,
                    last_name = contract.LastName,
                    order_summary = contract.OrderSummary
                };

                if (IsValid(email))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    await client.PostAsync(client.BaseAddress, content);
                    await _bluelightApiService.PostInvoice(contract.Invoice, contract.KinderlyIntegrationDate);
                    _logger.LogInformation("Customer with Email: {0} processed succesfully.", email);
                }
                else
                {
                    _logger.LogError("Customer with Email: {0} processed Unsuccesfully.", email);
                    failedContracts.Add(contract);
                }

                if (failedContracts.Any())
                    await _emailService.SendFailedContracts(failedContracts);
            }

        }

        public async Task SendApprovedPaceyMembersAsync()
        {
            var kinderlyMemberships = await ApprovePaceyMembers();

            if (kinderlyMemberships.Any())
            {
                await _bluelightApiService.PostMemberships(kinderlyMemberships);
                await _bluelightApiService.PostContacts(kinderlyMemberships);
            }
        }
    }
}
