namespace Middleware.Services.Cache
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CacheResponseAttribute : Attribute
    {
        public int Seconds { get; }

        public CacheResponseAttribute(int seconds = 60)
        {
            Seconds = seconds;
        }
    }
}