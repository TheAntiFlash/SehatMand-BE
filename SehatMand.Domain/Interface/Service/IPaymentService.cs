using Stripe;

namespace SehatMand.Domain.Interface.Service;

public interface IPaymentService
{
    public Task<string> CreateDoctorAccountAsync(string email, string name, string lastName);


    public Task<PaymentIntent> CreatePaymentIntentAsync(double amount, string doctorPaymentId, string appointmentId);
}