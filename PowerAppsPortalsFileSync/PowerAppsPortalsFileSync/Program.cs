using CommandLine;
using Newtonsoft.Json.Linq;
using PowerApps.Samples;
using PowerAppsPortalsFileSync.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
                   connectionString = $"Url={o.Url};Authority=null;ClientId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUrl=app://58145B91-0C36-4500-8554-080854F2AC97;UserPrincipalName={o.Email};Password={o.Password};CallerObjectId=null;Version=9.1;MaxRetries=3;TimeoutInSeconds=180;";

                   config = new ServiceConfig(connectionString);

                   try
                   {
                       processPortal(config, o.Folder, o.Save);

                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error: {ex.Message}");
                   }
               });

            Console.ReadLine();

        }

        private static void processPortal(ServiceConfig config, string baseFolder, bool import = false)
        {
            using (CDSWebApiService svc = new CDSWebApiService(config))
            {

                // Header required to include formatted values
                var formattedValueHeaders = new Dictionary<string, List<string>> {
                        { "Prefer", new List<string>
                            { "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"" }
                        }
                    };

                // Loading Languages
                Console.WriteLine("-- Start Loading Portal Languages --");
                var portalLanguages = (svc.Get("adx_portallanguages?" +
                    "$select=adx_portallanguageid,adx_languagecode",
                        formattedValueHeaders)["value"] as JArray).ToObject<PortalLanguage[]>();

                // Loading WebsiteLanguages
                Console.WriteLine("-- Start Loading WebSiteLanguages --");
                var languages = (svc.Get("adx_websitelanguages?" +
                "$select=adx_websitelanguageid,_adx_websiteid_value,_adx_portallanguageid_value,adx_name",
                    formattedValueHeaders)["value"] as JArray).ToObject<WebSiteLanguage[]>();

                // Loading WebSites
                Console.WriteLine("-- Start Loading WebSites --");
                var websites = (svc.Get("adx_websites?" +
                "$select=adx_websiteid,adx_name",
                    formattedValueHeaders)["value"] as JArray).ToObject<WebSite[]>();

                processWebsites(baseFolder, websites, portalLanguages, languages, import, svc);

            }
        }

        private static void processWebsites(string baseFolder, WebSite[] websites, PortalLanguage[] portalLanguages, WebSiteLanguage[] languages, bool import, CDSWebApiService svc)
        {
            // Start Creating Folder Structure
            foreach (var website in websites)
            {
                Console.WriteLine($"-- Start Creating Folder Structure for {website.Name} --");

                // Folder for WebSite
                var newWebSiteFolder = Helper.CreateFolder(baseFolder, Helper.ReplaceInvalidChars(website.Name));

                // Load Web Files for Website
                Console.WriteLine("-- Start Loading WebFiles --");
                var webfiles = (svc.Get("annotations?" +
                $"$select=annotationid,filename,_objectid_value,modifiedon,documentbody,mimetype,isdocument&$expand=objectid_adx_webfile($select=_adx_websiteid_value,adx_name)&$filter=(documentbody ne null and (endswith(filename, 'js') or endswith(filename, 'css'))) and objecttypecode eq 'adx_webfile' and isdocument eq true and (objectid_adx_webfile/_adx_websiteid_value eq {website.WebSiteId})")["value"] as JArray).ToObject<WebFile[]>(); 

                // WebFiles
                var dirInfoWebFiles = Directory.CreateDirectory(Path.Combine(Path.Combine(baseFolder, newWebSiteFolder), "WebFiles"));
                foreach (var webFile in webfiles)
                {
                    if (webFile.DocumentBody?.Length > 0)
                    {
                        try
                        {
                            //https://stackoverflow.com/questions/21733756/best-way-to-split-string-by-last-occurrence-of-character
                            string s = webFile.Filename;
                            int idx = s.LastIndexOf('.');

                            var fileName1Part = string.Empty;
                            var fileName2Part = string.Empty;


                            if (idx != -1)
                            {
                                fileName1Part = s.Substring(0, idx);
                                fileName2Part = s.Substring(idx + 1);


                                var webFilePath = Path.Combine(dirInfoWebFiles.FullName, $"{fileName1Part}-{webFile.AnnotationId}.{fileName2Part}");
                                if (!import)
                                {
                                    File.WriteAllBytes(webFilePath, Convert.FromBase64String(webFile.DocumentBody));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error WebFile: {webFile.Filename} Error: {ex.Message}");
                        }
                    }
                }

            }
        }


    }
}
