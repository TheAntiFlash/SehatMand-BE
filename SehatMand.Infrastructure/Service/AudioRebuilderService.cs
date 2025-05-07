using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class AudioRebuilderService(IStorageService storageService, IWebHostEnvironment env): IAudioRebuilderService
{
   public async Task<string> ProcessAudioAsync(string sid, string appointmentId)
   {
      var (m3U8File, m3U8Content) = await storageService.GetM3U8FileAsync(sid);
      using var tsStream = await storageService.GetMergedTsStreamAsync(m3U8Content, m3U8File);
      using var mp3Stream = await ConvertWithFFmpegInMemoryAsync(tsStream);
      var mp3FileName = $"{appointmentId}.mp3";
      var mp3FilePath = Path.Combine(env.WebRootPath, "audios", mp3FileName);
      await using var fileStream = new FileStream(mp3FilePath, FileMode.Create, FileAccess.Write);
      await mp3Stream.CopyToAsync(fileStream);
      return mp3FilePath.Split("wwwroot")[1];
   }
   private async Task<MemoryStream> ConvertWithFFmpegInMemoryAsync(Stream tsInput)
   {
      var outputStream = new MemoryStream();

      var processStartInfo = new ProcessStartInfo
      {
         FileName = "ffmpeg",
         Arguments = "-y -i pipe:0 -vn -acodec libmp3lame -f mp3 pipe:1",
         RedirectStandardInput = true,
         RedirectStandardOutput = true,
         RedirectStandardError = true,
         UseShellExecute = false,
         CreateNoWindow = true
      };

      var process = new Process { StartInfo = processStartInfo };
      process.Start();

      var inputTask = Task.Run(async () =>
      {
         await tsInput.CopyToAsync(process.StandardInput.BaseStream);
         process.StandardInput.Close();
      });

      var outputTask = Task.Run(async () =>
      {
         await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
      });

      string errorOutput = await process.StandardError.ReadToEndAsync();
      await Task.WhenAll(inputTask, outputTask);
      process.WaitForExit();

      if (process.ExitCode != 0)
         throw new Exception("FFmpeg failed: " + errorOutput);

      outputStream.Position = 0;
      return outputStream;
   }
}