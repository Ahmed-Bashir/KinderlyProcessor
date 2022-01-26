using KinderlyProcessor.Interfaces;
using KinderlyProcessor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace KinderlyProcessor.Services
{

    /// 
    /// <summary>
    /// 
    /// </summary>
    public class BluelightApiService : IBluelightApiService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        private string NewKinderlyMembersEndPoint { get; }
        private string ContactsEndPoint { get; }
        private string InvoiceEndPoint { get; }
        private ILogger<IBluelightApiService> _logger;




        public BluelightApiService(ILogger<IBluelightApiService> logger, IHttpClientFactory httpClientFactory, IConfiguration config )
        {
            _logger = logger;

            // End point to send members that have been approved by Kinderly
            NewKinderlyMembersEndPoint = "api/v1/Kinderly/Memberships";

            // End point to send contact informtion of members that have been approved by Kinderly
            ContactsEndPoint = "api/v1/Kinderly/Contacts";

            // End point to send Invoiveapproved by Kinderly
            InvoiceEndPoint = "api/v1/Kinderly/Invoices";

            _httpClientFactory = httpClientFactory;
            _config = config;
        }



        public async Task<List<Bluelight>> GetMembersAsync()
        {
            string dateFrom = DateTime.Today.AddDays(-14).ToString("yyyy-MM-dd");
            string dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            //string dateFrom = "2022-01-14";
            //string dateTo = "2022-01-17";

            var client = _httpClientFactory.CreateClient("BluelightApi");

            var response = await client.GetAsync(client.BaseAddress + $"api/v1/Kinderly/Memberships?dateFrom={dateFrom}&dateTo={dateTo}");
            if (!response.IsSuccessStatusCode) return new List<Bluelight>();
            _logger.LogInformation(await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<List<Bluelight>>();
        
        }

        public async Task<List<Bluelight>> GetContracts()
        {
            string dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
            string dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            //string dateFrom = "2022-01-17";
            //string dateTo = "2022-01-17";

            var client = _httpClientFactory.CreateClient("BluelightApi");

            var contractNames = _config.GetSection("Products").Get<string[]>();

            var productCodes = string.Join(",", contractNames.Where(s => !string.IsNullOrEmpty(s)));


            var response = await client.GetAsync(client.BaseAddress + $"api/v1/Kinderly/Invoices?dateFrom={dateFrom}&dateTo={dateTo}&productCodes={productCodes}");

            if (!response.IsSuccessStatusCode) return new List<Bluelight>();
            _logger.LogInformation(await response.Content.ReadAsStringAsync());

            
            return await response.Content.ReadFromJsonAsync<List<Bluelight>>();
        }




        public async Task PostInvoice(string id, string kinderlyIntegrationDate)
        {
            var client = _httpClientFactory.CreateClient("BluelightApi");

            var data = new
            {
                Id = id,

                KinderlyIntegrationDate = kinderlyIntegrationDate
            };

            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(client.BaseAddress + InvoiceEndPoint, content);
            var viewcontent = response.Content.ReadAsStringAsync();


        }


        public async Task PostMemberships(List<KinderlyMembership> kinderlyMembers)
        {
            var client = _httpClientFactory.CreateClient("BluelightApi");

            foreach (var member in kinderlyMembers)
            {
                var data = new
                {
                    member.Id,
                    member.KinderlyIntegrationDate
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(client.BaseAddress + NewKinderlyMembersEndPoint, content);
                var viewcontent = response.Content.ReadAsStringAsync();
            }

        }


        public async Task PostContacts(List<KinderlyMembership> kinderlyMembers)
        {
            var client = _httpClientFactory.CreateClient("BluelightApi");

            foreach (var member in kinderlyMembers)
            {
                var data = new
                {
                    member.Contact.Id,
                    member.KinderlyMember,
                    member.KinderlyLead
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(client.BaseAddress + ContactsEndPoint, content);



            }


        }


    }
}
