namespace Demo_RateLimiting.Middleware.Model
{
    public class ConfigurationModel
    {
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public int Period { get; set; }
        public int Limit { get; set; }
    }


}
