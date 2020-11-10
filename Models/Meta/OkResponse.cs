namespace Schlauchboot.Hosting.SoundCloud.Models.Meta
{
    public class OkResponse
    {
        public OkResponse(string okResponseMessage)
        {
            OkResponseMessage = okResponseMessage;
        }

        public string OkResponseMessage { get; }
    }
}
