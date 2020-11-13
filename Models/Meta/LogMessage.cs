using System.Collections.Generic;

namespace Schlauchboot.Hosting.SoundCloud.Models.Meta
{
    class LogMessage
    {
        public readonly Dictionary<string, string> logMessages = new Dictionary<string, string>()
        {
            { "requestReceived", "A Request has been received!" },
            { "requestMalformed", "The Request from was malformed!" },
            { "requestAuthentication", "The Request should have authenticated successfully!" },
            { "fileDownload", "The requested File has been downloaded successfully!" },
            { "fileDownloadPlaylist", "A requested File has been downloaded successfully!" },
            { "requestSuccess", "The Request was processed without Issues!" },
            { "requestError", "The Request was aborted due to:" },
            { "fileCleanup", "All associated Files have been cleaned up!" },
            { "fileUpload", "The Upload to the specified Hoster has been started!" },
            { "requestPlaylist", "The Reuquest is categorized as Playlist!" },
            { "playlistInformation", "The Information for the Playlist was gathered successfully!" },
            { "zipStatus", "All Files have been successfully zipped!" }
        };
    }
}
