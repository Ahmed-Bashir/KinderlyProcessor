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

            var paceyMembers = new List<PaceyMembership>();

            var kinderlyMemberships = new List<KinderlyMembership>();

            //Retrive a list of unapproved Pacey members from Bluelight 
            var members = await _bluelightApiService.GetMembersAsync();


            if (members.Count == 0)

                return kinderlyMemberships;

            // Creates a list of Pacey members with data from Bluelight using Json Model
            foreach (var member in members)
            {
                var paceyMember = new PaceyMembership()
                {

                    contact_id = member.Contact.Id,
                    email = member.Contact.EmailAddress1,
                    postcode = member.Contact.Address2Postcode,
                    first_name = member.Contact.FirstName,
                    last_name = member.Contact.LastName,

                    membership = new Membership()
                    {

                        membership_sub_type = member.SubType.Label,
                        membership_class = member.Class.Name,
                        membership_no = member.MembershipNumber,
                        membership_grade = member.Grade.Name,
                        membership_id = member.Id,
                        payment_method = member.PaymentMethod.Label,
                        membership_valid_from = member.ValidFrom.ToString(),
                        membership_valid_to = member.ValidTo.ToString()

                    }

                };

                paceyMembers.Add(paceyMember);

            }


         

            // Send Pacey memebers to Kinderly 
            var response = await client.PostAsJsonAsync(client.BaseAddress, new { data = paceyMembers });

            // Once approved, Kinderly returns the same list of members but with addtional information 
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(result);

         dynamic KinderlyResponse = JsonConvert.DeserializeObject(jsonResponse);

            // Use Kinderly response date to create a list of now Kinderly members using Json model 

            var unApprovedMembers = new List<dynamic>();
            string membershipId = string.Empty;
            string email = string.Empty;

            if (KinderlyResponse != null)
            {

                foreach (var member in KinderlyResponse.data)
                {


                    if (member.success == true)
                    {

                        membershipId = member.customer.membership_id;
                        email = member.customer.email;

                        var kinderlyMember = new KinderlyMembership()
                        {
                            Id = member.customer.membership_id,

                            Contact = new Contact()
                            {
                                Id = member.customer.contact_id
                            },

                            KinderlyLead = member.customer.kinderly_lead != "1" ? false : true,
                            KinderlyMember = member.customer.kt_member != "1" ? false : true
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
                foreach (var item in contract.Products)
                {
                    var orderSummary = new OrderSummary()
                    {

                        quantity = item.Quantity,
                        urid = item.Product.Name,
                        order_ref_id = contract.CmsId

                    };

                    orders.Add(orderSummary);
                }



                var digitalContract = new DigitalContract()
                {

                    invoice = contract.Products.Select(item => item.Invoice.Id).FirstOrDefault(),
                    email = contract.Contact.EmailAddress1,
                    first_name = contract.Contact.FirstName,
                    last_name = contract.Contact.LastName,
                    order_summary = orders



                };



                digitalContracts.Add(digitalContract);

            }



            foreach (var contract in digitalContracts)
            {
                foreach (var product in contract.order_summary)
                {
                    if (_applicationSetting.ContainsKey(product.urid))
                    {
                        product.urid = _applicationSetting[product.urid];
                    }
                    else 
                    {
                        unrecognisedProducts.Add(product.urid);
                        continue;
                        
                    }
                   
                    

                }

                if (unrecognisedProducts.Any())
                {

                    var item = new
                    {
                        contract.email,
                        contract.first_name,
                        contract.last_name,
                        contract.invoice,
                        products = unrecognisedProducts



                    };

                  await _emailService.SendUnrecognisedProducts(item);
                }


            }

           




            return digitalContracts;

        }


        public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

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

            if (digitalContracts.Any())
            {
                foreach (var contract in digitalContracts)
                {
                    var email = contract.email;
                    var data = new
                    {
                        email,
                        contract.first_name,
                        contract.last_name,
                        contract.order_summary
                    };

                    if (IsValid(email))
                    {
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(client.BaseAddress, content);
                        await _bluelightApiService.PostInvoice(contract.invoice, contract.KinderlyIntegrationDate);

                        _logger.LogInformation("Customer with Email: {0} processed succesfully.", email);
                    }
                    else
                    {
                        _logger.LogError("Customer with Email: {0} processed Unsuccesfully.", email);

                        failedContracts.Add(contract);

                      

                    }


                    if(failedContracts.Any())

                    await _emailService.SendFailedContracts(failedContracts);




                    // var viewcontent = response.Content.ReadAsStringAsync();


                }






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

