using Org.BouncyCastle.Utilities.Net;
using SehatMand.Domain.Interface.Service;
using Stripe;

namespace SehatMand.Infrastructure.Service;

public class StripeService: IPaymentService
{

   public async Task<string> CreateDoctorAccountAsync(string email, string name, string lastName)
   {
      var options = new AccountCreateOptions
      {
         TosAcceptance = new AccountTosAcceptanceOptions
         {
            Date = DateTime.UtcNow,
            Ip = "192.168.1.20"
         },
         Individual = new AccountIndividualOptions
         {
            SsnLast4 = "0000",
            FirstName = name,
            LastName = lastName,
            Email = email,
            Phone = "8888675309",
            Dob = new DobOptions
            {
               Day = 1,
               Month = 1,
               Year = 2000
            },
            Address = new AddressOptions
            {
               City = "New York City",
               Line1 = "Home",
               PostalCode = "10001",
               State = "New York"
            }
         },
         BusinessProfile = new AccountBusinessProfileOptions
         {
            Url = $"https://{email.Split('@')[0]}.com",
         },
         BusinessType = "individual",
         ExternalAccount = new AccountBankAccountOptions
         {
            Country = "US",
            Currency = "usd",
            AccountHolderName = "Jane Austen",
            AccountHolderType = "individual",
            RoutingNumber = "110000000",
            AccountNumber = "000123456789",
         },
         //Type = "express", // Use 'express' for a simplified onboarding process
         Country = "US", // Doctor's country
         Email = email, // Replace with the doctor's email
         Capabilities = new AccountCapabilitiesOptions
         {
            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            // BankTransferPayments = new AccountCapabilitiesBankTransferPaymentsOptions { Requested = true },
         },
         Controller = new AccountControllerOptions
         {
            Losses = new AccountControllerLossesOptions { Payments = "application" },
            Fees = new AccountControllerFeesOptions { Payer = "application" },
            StripeDashboard = new AccountControllerStripeDashboardOptions { Type = "none" },
            RequirementCollection = "application",
         }
      };
      var accountService = new AccountService();
      var account = await accountService.CreateAsync(options);
      return account.Id;
   }

   public async Task<PaymentIntent> CreatePaymentIntentAsync(double amount, string doctorPaymentId,
      string appointmentId)
   {
      var options = new PaymentIntentCreateOptions
      {
         Amount = (long)amount,
         Currency = "pkr",
         PaymentMethodTypes = new List<string> { "card" },
         CaptureMethod = "manual",
         Description = "Appointment Booking Payment",
         Metadata = new Dictionary<string, string>
         {
            { "appointment_id", appointmentId }, // Track your condition (e.g., appointment ID)
         },
         ApplicationFeeAmount = (long)(amount * 0.1), // 10% platform fee in cents
         TransferData = new PaymentIntentTransferDataOptions
         {
            Destination = doctorPaymentId // Connected account ID
         }
      };
      var service = new PaymentIntentService();
      var intent = await service.CreateAsync(options);
      return intent;
   } 
}