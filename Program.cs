using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Spotiloader
{
    public static class Program
    {
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddLogging(l => l.AddSerilog())
                .BuildServiceProvider();
        }
        
        public static void Main()
        {
            
        }
    }
}