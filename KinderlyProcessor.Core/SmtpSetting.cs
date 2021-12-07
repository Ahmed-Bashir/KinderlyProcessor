namespace KinderlyProcessor.Core
{
    public class SmtpSetting
    {
        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpUser { get; set; }

        public string SmtpPassword { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Cc { get; set; }

        public string Subject { get; set; }
    }
}
