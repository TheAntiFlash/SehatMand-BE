using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Service;

public interface IAgoraService
{

   // Task ScheduleRoom(string appointmentId, DateTime startTime);
   public string GenerateRtcToken(uint uid, string channel);
   
   public Task<string> AcquireCloudRecordingId(string channelName, uint recordingId = 3U);
   
   public Task<string> StartCloudRecording(string channelName, string resourceId, uint recordingId = 3U);

   public Task<(string rid, string sid)> Record(string cname);
   public Task<string> StopRecording(string cname, string rid, string sid, uint recordingId = 3);
}