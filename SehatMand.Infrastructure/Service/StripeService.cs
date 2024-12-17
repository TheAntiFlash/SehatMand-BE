using SehatMand.Domain.Interface.Service;
using Stripe;

namespace SehatMand.Infrastructure.Service;

public class StripeService: IPaymentService
{

   public async Task<string> CreateDoctorAccountAsync(string email)
   {
      var options = new AccountCreateOptions
      {
         Type = "express", // Use 'express' for a simplified onboarding process
         Country = "US", // Doctor's country
         Email = email, // Replace with the doctor's email
         Capabilities = new AccountCapabilitiesOptions
         {
            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
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
         Amount = (long)(amount * 100), // Convert to cents
         Currency = "usd",
         PaymentMethodTypes = new List<string> { "card" },
         CaptureMethod = "manual",
         Description = "Appointment Booking Payment",
         Metadata = new Dictionary<string, string>
         {
            { "appointment_id", appointmentId }, // Track your condition (e.g., appointment ID)
         },
         ApplicationFeeAmount = (long)(amount * 10), // 10% platform fee in cents
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