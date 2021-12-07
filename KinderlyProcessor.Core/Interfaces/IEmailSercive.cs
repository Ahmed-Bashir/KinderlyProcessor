using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinderlyProcessor.Core.Interfaces
{
    public interface IEmailSercive
    {
        Task SendUnapprovedMembers(List<dynamic> members);

        Task SendUnrecognisedProducts(dynamic item);

        Task SendFailedContracts(List<dynamic> members);
    }
}
