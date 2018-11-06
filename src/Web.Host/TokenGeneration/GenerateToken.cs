using System.Collections.Generic;

namespace Web.Host.TokenGeneration
{
    public class GenerateToken
    {
        public string Username { get; set; }
        public List<string> Audiences { get; set; }
        public string ClientId { get; set; }
    }
}