using KinderlyProcessor.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KinderlyProcessor.Core.Interfaces
{
    public interface IKinderlyApiService
    {
        Task<List<KinderlyMembership>> ApprovePaceyMembers();
        Task<List<DigitalContract>> GetDigitalContractsAsync();

        Task ProcessDigitalContractsAsync(string api);
        Task SendApprovedPaceyMembersAsync();
    }
}
