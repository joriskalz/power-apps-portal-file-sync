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
                   connectionString = $"Url={o.Url};Authority=null;UserPrincipalName={o.Email};Password={o.Password};CallerObjectId=null;Version=9.1;MaxRetries=3;TimeoutInSeconds=180;";
                   config = new ServiceConfig(connectionString);

                   try
                   {
                       
                   }
                   catch (Exception)
                   {
                       throw;
                   }
               });

        }
    }
}
