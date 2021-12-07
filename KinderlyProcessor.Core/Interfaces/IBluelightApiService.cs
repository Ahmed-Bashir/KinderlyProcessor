﻿using System.Collections.Generic;
using System.Threading.Tasks;
using KinderlyProcessor.Core.Models;

namespace KinderlyProcessor.Core.Interfaces
{
    public interface IBluelightApiService
    {
        Task<List<Bluelight>> GetMembersAsync();

        Task<List<Bluelight>> GetContracts();

        Task PostContacts(List<KinderlyMembership> kinderlyMembers);

        Task PostMemberships(List<KinderlyMembership> kinderlyMembers);

        Task PostInvoice(string id, string kinderlyIntegrationDate);
    }
}