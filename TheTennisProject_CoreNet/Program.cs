using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TheTennisProject_CoreNet
{
    /// <summary>
    /// Todo
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
