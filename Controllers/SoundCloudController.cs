using System;
using System.IO;
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
        private readonly Dictionary<string, string> _logMessages = new LogMessage().logMessages;
        private readonly string _tempFileStore = new Meta().GetAssemblyPath() + "\\TempFileStore";

        public SoundCloudController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostSoundCloud([FromBody]RequestBody requestBody)
        {
            var requestIp = Request.HttpContext.Connection.RemoteIpAddress;

            _logger.Information(string.Join(" ", requestIp, _logMessages["requestReceived"]));

            if (string.IsNullOrEmpty(requestBody.clientId) || string.IsNullOrEmpty(requestBody.soundcloudUrl.ToString()))
            {
                _logger.Information(string.Join(" ", requestIp, _logMessages["requestMalformed"]));

                return StatusCode(400, new ErrorResponse(_logMessages["requestMalformed"]));
            }
                    
            var soundCloudManager = new Manager.Methods.SoundCloud();

            var createdFiles = new List<string>();

            try
            {
                if (requestBody.soundcloudUrl.ToString().Contains("sets")) //Use regex later
                {
                    _logger.Information(string.Join(" ", requestIp, _logMessages["requestPlaylist"]));

                    var playlistInformation = soundCloudManager.ResolvePlaylistUrl(requestBody.soundcloudUrl, requestBody.clientId);

                    _logger.Information(string.Join(" ", requestIp, _logMessages["requestAuthentication"]));

                    var playlistFolder = Directory.CreateDirectory($"{_tempFileStore}\\{playlistInformation.Title}");

                    _logger.Information(string.Join(" ", requestIp, _logMessages["playlistInformation"]));

                    foreach (var track in playlistInformation.Tracks)
                    {
                        var trackUrl = soundCloudManager.QueryTrackUrl(track.Id, requestBody.clientId);
                        
                        var trackInformation = soundCloudManager.ResolveTrackUrl(trackUrl, requestBody.clientId);

                        var trackName = trackInformation.Title;

                        var trackMediaUrl = soundCloudManager.QueryTrackTranscodings(trackInformation.Media.Transcodings, requestBody.clientId);
                        
                        var filePath = $"{_tempFileStore}\\{playlistInformation.Title}\\{trackName}.mp3";

                        var trackM3u8 = soundCloudManager.QueryTrackM3u8(trackMediaUrl);

                        var ffmpegManager = new Ffmpeg();
                        await ffmpegManager.DownloadTrack(trackM3u8, filePath);

                        _logger.Information(string.Join(" ", requestIp, _logMessages["fileDownloadPlaylist"]));

                        var trackMediaInformationMod = new TrackInformationMod()
                        {
                            artist = trackInformation.User.Username,
                            album = trackInformation.User.Username,
                            title = trackInformation.Title
                        };

                        var id3Manager = new Id3();
                        id3Manager.SetFileMetadata(filePath, trackMediaInformationMod);
                    }

                    string zipPath = $"{_tempFileStore}\\{playlistInformation.Title}.zip";

                    var compressionManager = new Compression();
                    compressionManager.CompressFolder(playlistFolder.FullName, zipPath);

                    _logger.Information(string.Join(" ", requestIp, _logMessages["zipStatus"]));

                    var fileStream = await System.IO.File.ReadAllBytesAsync(zipPath);

                    Response.Headers.Add("File-Type", "zip");

                    createdFiles.Add(zipPath);
                    createdFiles.Add(playlistFolder.FullName);

                    return File(fileStream, "application/octet-stream");
                }
                else
                {
                    var trackInformation = soundCloudManager.ResolveTrackUrl(requestBody.soundcloudUrl, requestBody.clientId);

                    _logger.Information(string.Join(" ", requestIp, _logMessages["requestAuthentication"]));

                    string trackName = trackInformation.Title;

                    var trackMediaInformation = soundCloudManager.QueryTrackTranscodings(trackInformation.Media.Transcodings,
                        requestBody.clientId);

                    var trackMediaUrl = soundCloudManager.QueryTrackM3u8(trackMediaInformation);

                    string filePath = $"{_tempFileStore}\\{trackName}.mp3";

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

                    Response.Headers.Add("File-Type", "mp3");

                    return File(fileStream, "application/octet-stream");
                }
            }
            catch (Exception processException)
            {
                _logger.Information(string.Join(" ", requestIp, _logMessages["requestError"], processException.Message));

                return StatusCode(500, new ErrorResponse(processException.Message));
            }
            finally
            {
                foreach (var file in createdFiles)
                {
                    if (Directory.Exists(file))
                    {
                        Directory.Delete(file, true);
                    }
                    else
                    {
                        System.IO.File.Delete(file);
                    }
                }
                //Leaves still some unknow file behind when downloading singular Tracks...

                _logger.Information(string.Join(" ", requestIp, _logMessages["fileCleanup"]));
            }
        }
    }
}
