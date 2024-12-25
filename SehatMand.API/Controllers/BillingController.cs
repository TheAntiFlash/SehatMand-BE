using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using Stripe;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController: ControllerBase
{
   
    [Authorize]
    [HttpGet]
    [Route("received-details")]
    public async Task<IActionResult> GetReceivedDetails()
    {
        try
        {
            var options = new BalanceGetOptions(); // No additional filters needed
            var requestOptions = new RequestOptions
            {
                StripeAccount = "acct_1QYoOGGgI82s8f0J", // Connected account ID (acct_XXXXXX)
            };

            var service = new BalanceService();
            var balance =  await service.GetAsync(options, requestOptions);
            return Ok(balance);

        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "An error occurred while fetching received details:", "" +e.Message));
        }
        
    }
    
}