using socket_worker.Services;

namespace socket_worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<WorkerService>();

            var host = builder.Build();
            host.Run();
        }
    }
}