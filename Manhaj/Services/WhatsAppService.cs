using System.Text;
using System.Text.Json;

namespace Manhaj.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WhatsAppService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> SendMessageAsync(string to, string message)
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages";

            // Normalize phone number
            // Remove any non-digit characters
            var digitsOnly = new string(to.Where(char.IsDigit).ToArray());

            // Handle Egyptian numbers
            if (digitsOnly.StartsWith("01"))
            {
                to = "20" + digitsOnly.Substring(1);
            }
            else if (digitsOnly.StartsWith("1") && digitsOnly.Length == 10)
            {
                to = "20" + digitsOnly;
            }
            else if (digitsOnly.StartsWith("20"))
            {
                to = digitsOnly;
            }
            else
            {
                // Fallback or other country codes
                to = digitsOnly;
            }

            Console.WriteLine($"Sending WhatsApp to: {to}");

            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "text",
                text = new { body = message }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            Console.WriteLine($"WhatsApp Payload: {jsonPayload}");

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"WhatsApp API Error: {errorContent}");
                }
                else 
                {
                    Console.WriteLine("WhatsApp Message Sent Successfully");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WhatsApp Exception: {ex.Message}");
                return false;
            }
        }
    }
}
