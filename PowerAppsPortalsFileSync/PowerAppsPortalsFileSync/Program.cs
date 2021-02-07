using CommandLine;
using PowerApps.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync
{
    class Program
    {
        static string connectionString = string.Empty;
        static ServiceConfig config;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(o =>
               {
                   connectionString = $"Url={o.Url};Authority=null;UserPrincipalName={o.Email};Password={o.Password};ClientId=12f81219-12ee-4a9e-12ae-a2591f43381d;RedirectUrl=app://12f81219-12ee-4a9e-12ae-a2591f43381d;CallerObjectId=null;Version=9.1;MaxRetries=3;TimeoutInSeconds=180;";
                   config = new ServiceConfig(connectionString);

                   try
                   {
                       
                   }
                   catch (Exception)
                   {
                       throw;
                   }
               });

            Console.ReadLine();

        }
    }
}
