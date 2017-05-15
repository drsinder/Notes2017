/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: MessageServices.cs
**
**  Description:
**      Message Services - Email/SMS
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2017.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.

            var apiKey = App_Code.Global.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(App_Code.Global.SendGridEmail, App_Code.Global.EmailName);
            var to = new EmailAddress(email);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            //if (message.StartsWith("<!DOCTYPE html>"))
            //{
            //    MemoryStream ms = new MemoryStream();
            //    StreamWriter sw = new StreamWriter(ms);
            //    sw.Write(message);
            //    sw.Flush();
            //    ms.Seek(0, SeekOrigin.Begin);

            //    msg.AddAttachment(ms, "Notes20171.html");
            //}

            var response = await client.SendEmailAsync(msg);

            return;
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            TwilioClient.Init(App_Code.Global.TwilioName, App_Code.Global.TwilioPassword);

            var msg = MessageResource.Create(
                 to: new PhoneNumber(number),
                from: new PhoneNumber(App_Code.Global.TwilioNumber),
                body: message);

            return Task.FromResult(0);

        }
    }
}
