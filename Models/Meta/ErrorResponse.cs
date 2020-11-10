namespace Schlauchboot.Hosting.SoundCloud.Models.Meta
{
    class ErrorResponse
    {
        public ErrorResponse(string errorResponseMessage)
        {
            ErrorResponseMessage = errorResponseMessage;
        }

        public string ErrorResponseMessage { get; }
    }
}
