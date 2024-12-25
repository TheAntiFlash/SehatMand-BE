using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Smtp;

namespace SehatMand.Infrastructure.Service;

public class EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger): IEmailService
{
    
    public async Task SendOtpEmailAsync(string email, string otp, string firstName, int expireInMinutes)
    {
        logger.LogInformation($"{smtpSettings.Value.Username} : {smtpSettings.Value.Password}");
        using var smtpClient = new SmtpClient(smtpSettings.Value.Host, 587);
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Value.Username, smtpSettings.Value.Password);
        var mailMessage = new MailMessage(smtpSettings.Value.Username, email)
        {
            Subject = "SehatMand Registration OTP Code",
            IsBodyHtml = true,
            Body = $@"<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"" />
    <title>Static Template</title>

    <link
      href=""https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600&display=swap""
      rel=""stylesheet""
    />
  </head>
  <body
    style=""
      margin: 0;
      font-family: 'Poppins', sans-serif;
      background: #ffffff;
      font-size: 14px;
    ""
  >
    <div
      style=""
        max-width: 680px;
        margin: 0 auto;
        padding: 45px 30px 60px;
        background: #f4f7ff;
        background-image: url(https://archisketch-resources.s3.ap-northeast-2.amazonaws.com/vrstyler/1661497957196_595865/email-template-background-banner);
        background-repeat: no-repeat;
        background-size: 800px 452px;
        background-position: top center;
        font-size: 14px;
        color: #434343;
      ""
    >
      <header>
        <table style=""width: 100%;"">
          <tbody>
            <tr style=""height: 0;"">
              <td>
                <!--<img
                  alt=""""
                  src=""https://archisketch-resources.s3.ap-northeast-2.amazonaws.com/vrstyler/1663574980688_114990/archisketch-logo""
                  height=""30px""
                />-->
              </td>
              <td style=""text-align: right;"">
                <span
                  style=""font-size: 16px; line-height: 30px; color: #ffffff;""
                  >{DateTime.Now.ToLongDateString()}</span
                >
              </td>
            </tr>
          </tbody>
        </table>
      </header>

      <main>
        <div
          style=""
            margin: 0;
            margin-top: 70px;
            padding: 92px 30px 115px;
            background: #ffffff;
            border-radius: 30px;
            text-align: center;
          ""
        >
          <div style=""width: 100%; max-width: 489px; margin: 0 auto;"">
            <h1
              style=""
                margin: 0;
                font-size: 24px;
                font-weight: 500;
                color: #1f1f1f;
              ""
            >
              Your OTP
            </h1>
            <p
              style=""
                margin: 0;
                margin-top: 17px;
                font-size: 16px;
                font-weight: 500;
              ""
            >
              Hey {firstName},
            </p>
            <p
              style=""
                margin: 0;
                margin-top: 17px;
                font-weight: 500;
                letter-spacing: 0.56px;
              ""
            >
              Use the following OTP to Login to the SehatMand App. OTP is
              valid for
              <span style=""font-weight: 600; color: #1f1f1f;"">{expireInMinutes} minutes</span>.
              Do not share this code with others.
            </p>
            <p
              style=""
                margin: 0;
                margin-top: 60px;
                font-size: 40px;
                font-weight: 600;
                letter-spacing: 25px;
                color: #ba3d4f;
              ""
            >
              {otp}
            </p>
          </div>
        </div>
        <!--<p
          style=""
            max-width: 400px;
            margin: 0 auto;
            margin-top: 90px;
            text-align: center;
            font-weight: 500;
            color: #8c8c8c;
          ""
        >
          Need help? Ask at
          <a
            href="" ""
            style=""color: #499fb6; text-decoration: none;""
            >archisketch@gmail.com</a
          >
          or visit our
          <a
            href=""""
            target=""_blank""
            style=""color: #499fb6; text-decoration: none;""
            >Help Center</a
          >
        </p> -->
      </main>

      <footer
        style=""
          width: 100%;
          max-width: 490px;
          margin: 20px auto 0;
          text-align: center;
          border-top: 1px solid #e6ebf1;
        ""
      >
        <p
          style=""
            margin: 0;
            margin-top: 40px;
            font-size: 16px;
            font-weight: 600;
            color: #434343;
          ""
        >
          SehatMand
        </p>
        <p style=""margin: 0; margin-top: 16px; color: #434343;"">
          Copyright © 2024 SehatMand. All rights reserved.
        </p>
      </footer>
    </div>
  </body>
</html>
" 
        };
        await smtpClient.SendMailAsync(mailMessage);
    }
}