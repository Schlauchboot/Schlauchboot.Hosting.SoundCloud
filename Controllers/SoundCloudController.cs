using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

using Serilog;

using Schlauchboot.Hosting.SoundCloud.Models.Meta;
using Schlauchboot.Hosting.SoundCloud.Manager.Methods;
using Schlauchboot.Hosting.SoundCloud.Manager.Models.SoundCloud;

namespace Schlauchboot.Hosting.SoundCloud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoundCloudController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, string> _logMessages = new Dictionary<string, string>()
        {
            { "requestReceived", "A Request has been received!" },
            { "requestMalformed", "The Request from was malformed!" },
            { "requestAuthentication", "The Request should have authenticated successfully!" },
            { "fileDownload", "The requested File has been downloaded successfully!" },
            { "requestSuccess", "The Request was processed without Issues!" },
            { "requestError", "The Request was aborted due to:" },
            { "fileCleanup", "All associated Files have been cleaned up!" },
            { "fileUpload", "The Upload to the specified Hoster has been started!" }
        };

        public SoundCloudController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostSoundCloud([FromBody]RequestBody requestBody)
        {
            var requestIp = Request.HttpContext.Connection.RemoteIpAddress;

            _logger.Information(string.Join(" ", requestIp, _logMessages["requestReceived"]));

            if (string.IsNullOrEmpty(requestBody.clientId) || string.IsNullOrEmpty(requestBody.trackUrl))
            {
                _logger.Information(string.Join(" ", requestIp, _logMessages["requestMalformed"]));

                return StatusCode(400, new ErrorResponse(_logMessages["requestMalformed"]));
            }

            var soundCloudManager = new Manager.Methods.SoundCloud();
            string trackName = string.Empty;
            string filePath = string.Empty;

            try
            {
                var trackInformation = soundCloudManager.ResolveTrackUrl(requestBody.trackUrl, requestBody.clientId);
                trackName = trackInformation.Title;

                _logger.Information(string.Join(" ", requestIp, _logMessages["requestAuthentication"]));

                var trackMediaInformation = soundCloudManager.QueryTrackMediaInformation(trackInformation.Media.Transcodings,
                    requestBody.clientId);

                var trackMediaUrl = soundCloudManager.QueryTrackMediaUrl(trackMediaInformation);

                filePath = $"C:\\Temp\\{trackName}.mp3";

                var ffmpegManager = new Ffmpeg();
                await ffmpegManager.DownloadTrack(trackMediaUrl, filePath);

                _logger.Information(string.Join(" ", requestIp, _logMessages["fileDownload"]));

                var trackMediaInformationMod = new TrackInformationMod()
                {
                    artist = trackInformation.User.Username,
                    album = trackInformation.User.Username,
                    title = trackInformation.Title
                };

                var id3Manager = new Id3();
                id3Manager.SetFileMetadata(filePath, trackMediaInformationMod);

                var fileStream = await System.IO.File.ReadAllBytesAsync(filePath);

                _logger.Information(string.Join(" ", requestIp, _logMessages["requestSuccess"]));

                return File(fileStream, "application/octet-stream");
            }
            catch (Exception processException)
            {
                _logger.Information(string.Join(" ", requestIp, _logMessages["requestError"], processException.Message));

                return StatusCode(500, new ErrorResponse(processException.Message));
            }
            finally
            {
                if (System.IO.File.Exists($"C:\\Temp\\{trackName}.mp3"))
                {
                    System.IO.File.Delete($"C:\\Temp\\{trackName}.mp3");

                    _logger.Information(string.Join(" ", requestIp, _logMessages["fileCleanup"]));
                }
            }
        }
    }
}
