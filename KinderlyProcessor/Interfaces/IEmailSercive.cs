using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KinderlyProcessor.Interfaces
{
  public interface IEmailSercive
    {

        Task SendUnapprovedMembers(List<dynamic> members);
        Task SendUnrecognisedProducts(dynamic item);
        Task SendFailedContracts(List<dynamic> members);
    }
}
