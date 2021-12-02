
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Text;
using FluentEmail.Razor;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using KinderlyProcessor.Core.Interfaces;

namespace KinderlyProcessor.Core.Services
{
    public class EmailService : IEmailSercive
    {

        private readonly SmtpSetting smtpSetting; 

        public EmailService(IOptions<SmtpSetting> options)
        {
            smtpSetting = options.Value;
        }

     
        public async Task SendUnapprovedMembers(List<dynamic> members)
        {


            Email.DefaultSender = GetSmtpSender();
            Email.DefaultRenderer = new RazorRenderer();

            var template = new StringBuilder ();

           
           template.AppendLine("The following members were not approved:");
           template.AppendLine("@foreach (var member in Model)" +
                "{" +
                "<p>Membership ID: @member.customer.membership.membership_id </p>" +
                "<p>First name: @member.customer.first_name </p>" +
                "<p>Last name: @member.customer.last_name </p>" +
                 "<p>Error:</p>" +
                "@foreach (var error in member.message.postcode)" +
                 "{" +
                 "<ul><li> @error </li></ul>" +
                  "}" +
                "}");

            var email = await Email
                .From(smtpSetting.From)
                .To(smtpSetting.To)
                .CC(smtpSetting.CC)
                .Subject(smtpSetting.Subject)
                .UsingTemplate(template.ToString(), members)
                .SendAsync();


        }

        public async Task SendFailedContracts(List<dynamic> contracts)
        {
            

            Email.DefaultSender = GetSmtpSender();
            Email.DefaultRenderer = new RazorRenderer();

            var template = new StringBuilder();


            template.AppendLine("The following contract(s) could not be processed:");
            template.AppendLine("@foreach (var contract in Model)" +
                 "{" +
                 "<p>Email: @contract.email </p>" +
                 "<p>First name: @contract.first_name </p>" +
                 "<p>Last name: @contract.last_name </p>" +
                 "<p>Invoice: @contract.invoice </p>" +
                 "}");

            var email = await Email
                .From(smtpSetting.From)
                .To(smtpSetting.To)
                .CC(smtpSetting.CC)
                .Subject(smtpSetting.Subject)
                .UsingTemplate(template.ToString(), contracts)
                .SendAsync();


        }


        public async Task SendUnrecognisedProducts(dynamic item)
        {


            Email.DefaultSender = GetSmtpSender();
            Email.DefaultRenderer = new RazorRenderer();

            var template = new StringBuilder();


            template.AppendLine("The following product(s) could not be recognised:");
            template.AppendLine(
                 "<p>Email: @Model.email </p>" +
                 "<p>First name: @Model.first_name </p>" +
                 "<p>Last name: @Model.last_name </p>" +
                 "<p>Invoice: @Model.invoice </p>" +
                  "<p>Error:</p>" +
                 "@foreach (var error in Model.products)" +
                  "{" +
                  "<ul><li> @error </li></ul>" +
                   "}");

            var email = await Email
                .From(smtpSetting.From)
                .To(smtpSetting.To)
                .CC(smtpSetting.CC)
                .Subject(smtpSetting.Subject)
                .UsingTemplate(template.ToString(), item)
                .SendAsync();


        }

        private SmtpSender GetSmtpSender()
        {
            var sender = new SmtpSender(() => new SmtpClient(smtpSetting.SmtpHost)

            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential()
                {
                    UserName = smtpSetting.SmtpUser,
                    Password = smtpSetting.SmtpPassword,

                },
                Port = smtpSetting.SmtpPort,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,

            });

            return sender;
        }

    }
}
