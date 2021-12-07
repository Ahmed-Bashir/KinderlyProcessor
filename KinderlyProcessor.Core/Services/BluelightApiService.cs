﻿using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using KinderlyProcessor.Core.Models;
using KinderlyProcessor.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace KinderlyProcessor.Core.Services
{
    public class BluelightApiService : IBluelightApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private string NewKinderlyMembersEndPoint { get; }

        private string ContactsEndPoint { get; }

        private string InvoiceEndPoint { get; }

        public BluelightApiService(IHttpClientFactory httpClientFactory)
        {
            // End point to send members that have been approved by Kinderly
            NewKinderlyMembersEndPoint = "api/v1/Kinderly/Memberships";

            // End point to send contact informtion of members that have been approved by Kinderly
            ContactsEndPoint = "api/v1/Kinderly/Contacts";

            // End point to send Invoiveapproved by Kinderly
            InvoiceEndPoint = "api/v1/Kinderly/Invoices";

            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Bluelight>> GetMembersAsync()
        {
            var dateFrom = DateTime.Today.AddDays(-14).ToString("yyyy-MM-dd");
            var dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            var client = _httpClientFactory.CreateClient("BluelightApi");
            var response = await client.GetAsync(client.BaseAddress + $"api/v1/Kinderly/Memberships?dateFrom={dateFrom}&dateTo={dateTo}");
            return await response.Content.ReadFromJsonAsync<List<Bluelight>>();
        }

        public async Task<List<Bluelight>> GetContracts()
        {
            string dateFrom = DateTime.Now.ToString("yyyy-MM-dd");
            string dateTo = DateTime.Now.ToString("yyyy-MM-dd");
            var client = _httpClientFactory.CreateClient("BluelightApi");
            var response = await client.GetAsync(client.BaseAddress + $"api/v1/Kinderly/Invoices?dateFrom={dateFrom}&dateTo={dateTo}&productCodes=CC1D,WCC1D,NCC1D,STCCD,ASCC1D");
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
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
            var viewcontent = await response.Content.ReadAsStringAsync();
        }

        public async Task PostMemberships(List<KinderlyMembership> kinderlyMembers)
        {
            var client = _httpClientFactory.CreateClient("BluelightApi");

            foreach (var content in kinderlyMembers.Select(member => new
            {
                member.Id,
                member.KinderlyIntegrationDate
            }).Select(data => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")))
            {
                var response = await client.PostAsync(client.BaseAddress + NewKinderlyMembersEndPoint, content);
                var viewcontent = await response.Content.ReadAsStringAsync();
            }
        }

        public async Task PostContacts(List<KinderlyMembership> kinderlyMembers)
        {
            var client = _httpClientFactory.CreateClient("BluelightApi");

            foreach (var content in kinderlyMembers.Select(member => new
            {
                member.Contact.Id,
                member.KinderlyMember,
                member.KinderlyLead
            }).Select(data => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")))
            {
                var response = await client.PostAsync(client.BaseAddress + ContactsEndPoint, content);
            }
        }
    }
}
