namespace BookFiy.Application.Settings
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        // If true use SslOnConnect (port 465); otherwise use StartTls
        public bool UseSslOnConnect { get; set; } = false;
    }
}
