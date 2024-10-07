using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Dto.Otp;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Service;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/otp")]
/***
 * This controller is responsible for handling all the OTP related requests.
 * It includes the following functionalities:
 * - Generate OTP
 * - Verify OTP
 */
public class OtpController(
    IOtpService otpService,
    ILogger<OtpController> logger,
    IUserRepository userRepo
    ): ControllerBase
{
    
    /// <summary>
    /// Generate OTP
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{email}")]
    public async Task<IActionResult> GenerateOtp([FromRoute] string email)
    {
        try
        {
            var user = await userRepo.GetByEmailAsync(email);
            if (user is null)
            {
                return NotFound(new ResponseDto(
                    "User Not Found",
                    "User not found for otp generation. Register again"
                ));
            }
            
            var otp = OtpService.GenerateOtp(length: 4);
            var otpExpireInMinutes = 10;
            await otpService.SendOtp(otp, user.Email, user.Email.Split("@")[0], otpExpireInMinutes); // replace in real user.Email 
            await otpService.SaveOtpForUserAsync(user.Id, otp, otpExpireInMinutes);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError("{name} Error while generating OTP. Error: " + e.Message, "OTP");
            return BadRequest(new ResponseDto(
                "Error",
                e.Message
            ));
        }
    }

    /// <summary>
    /// Verify OTP
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{email}/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto, [FromRoute] string email)
    {
        try
        {
            var user = await userRepo.GetByEmailAsync(email);
            if (user is null)
            {
                return NotFound(new ResponseDto(
                    "User Not Found",
                    "User not found for otp generation. Register again"
                ));
            }
            var verified = await otpService.VerifyOtpAsync(user.Id, dto.Otp);
            if (!verified)
            {
                return Unauthorized(new ResponseDto(
                    "Invalid OTP",
                    "otp is invalid or has expired. Re-generate OTP and Try Again."
                ));
            }

            await userRepo.Activate(user.Id);
            return Ok(new ResponseDto("Success", "OTP Verified Successfully!\nYou can login now."));
        }
        catch (Exception e)
        {
            logger.LogError("{name} Error while verifying OTP. Error: " + e.Message, "OTP");
            return BadRequest(new ResponseDto(
                "Error",
                e.Message
            ));
        }
    }
 
}