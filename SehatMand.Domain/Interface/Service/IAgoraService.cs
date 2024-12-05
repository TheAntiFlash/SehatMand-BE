using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Service;

public interface IAgoraService
{

   Task ScheduleRoom(string appointmentId, DateTime startTime);
   public string GenerateRtcToken(uint uid, string channel);

}