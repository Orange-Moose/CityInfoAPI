using CityInfo.API.Interfaces;

namespace CityInfo.API.Services
{
    public class LocalMailService: IMailService
    {
        private readonly string _mailTo = string.Empty; // setting field to empty string to be sure that values are adde only via constructor
        private readonly string _mailFrom = string.Empty;

        public LocalMailService(IConfiguration configuration) // injecting mail config from appsettings.json
        {
            _mailTo = configuration["mailSettings:mailTo"];
            _mailFrom = configuration["mailSettings:mailFrom"];
        }

        public void Send(string subject, string message)
        {
            Console.WriteLine($"Mail from: {_mailFrom} to {_mailTo} via {nameof(LocalMailService)}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
