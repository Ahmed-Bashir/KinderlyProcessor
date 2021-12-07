using KinderlyProcessor.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinderlyProcessor.Core.Interfaces
{
    public interface IKinderlyApiService
    {
        Task<List<KinderlyMembership>> ApprovePaceyMembers();

        Task<List<DigitalContract>> GetDigitalContractsAsync();

        Task ProcessDigitalContractsAsync();

        Task SendApprovedPaceyMembersAsync();
    }
}
