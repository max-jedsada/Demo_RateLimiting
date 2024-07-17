namespace Demo_RateLimiting.Middleware.Model
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LimitRequests : Attribute
    {
        public int Limit { get; set; }
        public int Period { get; set; }
    }


}
