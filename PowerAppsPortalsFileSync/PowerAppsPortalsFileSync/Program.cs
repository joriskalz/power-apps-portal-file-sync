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
            var countUpdatedFiles = 0;
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
                                else
                                {
                                    if (File.Exists(webFilePath))
                                    {
                                        var webFileDisk = File.ReadAllBytes(webFilePath);
                                        if (webFileDisk?.Length > 0)
                                        {
                                            var webFileOnline = Convert.FromBase64String(webFile.DocumentBody);

                                            if (!Helper.ByteArrayCompare(webFileOnline, webFileDisk))
                                            {
                                                Console.WriteLine("Changes detected in WebFile: " + webFile.Filename);

                                                // Update a webfile aka annotations
                                                JObject webfile = new JObject
                                                {
                                                    { "documentbody", Convert.ToBase64String(webFileDisk)  },
                                                    { "mimetype", webFile.MimeType  }
                                                };

                                                var webfileuri = new Uri($"{svc.BaseAddress}annotations({webFile.AnnotationId })");
                                                svc.Patch(webfileuri, webfile);
                                                countUpdatedFiles++;
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error WebFile: {webFile.Filename} Error: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine("-- Start Loading WebPages --");
                var webpages = (svc.Get("adx_webpages?" +
                $"$select=adx_webpageid,adx_name,adx_partialurl,adx_isroot,_adx_webpagelanguageid_value,_adx_parentpageid_value,_adx_websiteid_value,adx_copy,adx_customcss,adx_customjavascript&$filter=(adx_websiteid/adx_websiteid eq {website.WebSiteId})&$orderby=adx_isroot desc,adx_name asc")["value"] as JArray).ToObject<WebPage[]>();
        
                // Webpages
                var dirInfoWebPages = Directory.CreateDirectory(Path.Combine(Path.Combine(baseFolder, newWebSiteFolder), "WebPages"));
                foreach (var page in webpages)
                {
                    // new path for website and the language
                    var validName = Helper.ReplaceInvalidChars(page.Name);
                    var currentPath = Helper.CreateFolder(dirInfoWebPages.FullName, validName);

                    try
                    {
                        var portalLanguage = portalLanguages.SingleOrDefault(s => s.PortalLanguageId == languages.SingleOrDefault(t => t.WebSiteLanguageId == page.WebPageLanguageId).PortalLanguageId);
                        currentPath = Helper.CreateFolder(currentPath, portalLanguage.LanguageCode);
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("Error Detected: " + page.Name);
                        // Probalbly no language
                    }

                    var pathContent = Path.Combine(currentPath, $"{validName}-Content-{page.WebPageId}.html");
                    var pathCss = Path.Combine(currentPath, $"{validName}-CustomCss-{page.WebPageId}.css");
                    var pathJavascript = Path.Combine(currentPath, $"{validName}-CustomJavascript-{page.WebPageId}.js");

                    if (import)
                    {
                        try
                        {
                            var content = string.Empty;
                            var css = string.Empty;
                            var javascript = string.Empty;

                            if (File.Exists(pathContent))
                            {
                                content = File.ReadAllText(pathContent);
                                if (page.Copy?.Length > 0)
                                    if (page.Copy != content)
                                    {
                                        Console.WriteLine("Changes detected in content: " + page.Name);

                                        //Update a contact
                                        JObject webpage = new JObject
                                            {
                                                { "adx_copy", content }
                                            };

                                        var webpageuri = new Uri($"{svc.BaseAddress}adx_webpages({page.WebPageId})");
                                        svc.Patch(webpageuri, webpage);
                                        countUpdatedFiles++;
                                    }
                            }

                            if (File.Exists(pathCss))
                            {
                                css = File.ReadAllText(pathCss);
                                if (page.CustomCss?.Length > 0)
                                    if (page.CustomCss != css)
                                    {
                                        Console.WriteLine("Changes detected in css: " + page.Name);

                                        //Update a Css
                                        JObject webpage = new JObject
                                            {
                                                { "adx_customcss", css }
                                            };

                                        var webpageuri = new Uri($"{svc.BaseAddress}adx_webpages({page.WebPageId})");
                                        svc.Patch(webpageuri, webpage);
                                        countUpdatedFiles++;
                                    }
                            }
                            if (File.Exists(pathJavascript))
                            {
                                javascript = File.ReadAllText(pathJavascript);
                                if (page.CustomJavascript?.Length > 0)
                                    if (page.CustomJavascript != javascript)
                                    {
                                        Console.WriteLine("Changes detected in javascripts: " + page.Name);

                                        //Update a Javascript
                                        JObject webpage = new JObject
                                            {
                                                { "adx_customjavascript", javascript }
                                            };

                                        var webpageuri = new Uri($"{svc.BaseAddress}adx_webpages({page.WebPageId})");
                                        svc.Patch(webpageuri, webpage);
                                        countUpdatedFiles++;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (page.Copy?.Length > 0)
                        {
                            File.WriteAllText(pathContent, page.Copy);
                        }

                        if (page.CustomCss?.Length > 0)
                        {
                            File.WriteAllText(pathCss, page.CustomCss);
                        }

                        if (page.CustomJavascript?.Length > 0)
                        {
                            File.WriteAllText(pathJavascript, page.CustomJavascript);
                        }
                    }
                }

                Console.WriteLine("-- Start Loading WebTemplates --");
                var webTemplates = (svc.Get("adx_webtemplates?" +
                $"$select=adx_webtemplateid,adx_name,adx_source&$filter=(adx_websiteid/adx_websiteid eq {website.WebSiteId})&$orderby=adx_name asc")["value"] as JArray).ToObject<WebTemplate[]>();

                // Web Templates
                var dirInfoWebTemplates = Directory.CreateDirectory(Path.Combine(Path.Combine(baseFolder, newWebSiteFolder), "WebTemplates"));
                foreach (var template in webTemplates)
                {
                    // new path for website and the language
                    var validName = Helper.ReplaceInvalidChars(template.Name);
                    var currentPath = Helper.CreateFolder(dirInfoWebTemplates.FullName, validName);

                    var pathContent = Path.Combine(currentPath, $"{validName}-{template.WebTemplateId}.html");

                    if (import)
                    {
                        try
                        {
                            var content = string.Empty;

                            if (File.Exists(pathContent))
                            {
                                content = File.ReadAllText(pathContent);
                                if (template.Source?.Length > 0)
                                    if (template.Source != content)
                                    {
                                        Console.WriteLine("Changes detected in content: " + template.Name);

                                        // Update a webtemplate
                                        JObject webtemplate = new JObject
                                            {
                                                { "adx_source", content }
                                            };

                                        var webpageuri = new Uri($"{svc.BaseAddress}adx_webtemplates({template.WebTemplateId})");
                                        svc.Patch(webpageuri, webtemplate);
                                        countUpdatedFiles++;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (template.Source?.Length > 0)
                        {
                            File.WriteAllText(pathContent, template.Source);
                        }
                    }
                }

                Console.WriteLine("-- Start Loading ContentSnippets --");
                var contentSnippets = (svc.Get("adx_contentsnippets?" +
                $"$select=adx_name,_adx_contentsnippetlanguageid_value,adx_contentsnippetid,adx_value&$filter=(_adx_websiteid_value eq {website.WebSiteId})&$orderby=adx_name asc")["value"] as JArray).ToObject<ContentSnippet[]>();

                // ContentSnippets
                var dirInfoContentSnippets = Directory.CreateDirectory(Path.Combine(Path.Combine(baseFolder, newWebSiteFolder), "ContentSnippets"));
                foreach (var snippet in contentSnippets)
                {

                    // new path for website and the language
                    var validName = Helper.ReplaceInvalidChars(snippet.Name);
                    var currentPath = Helper.CreateFolder(dirInfoContentSnippets.FullName, validName);

                    try
                    {
                        var portalLanguage = portalLanguages.SingleOrDefault(s => s.PortalLanguageId == languages.SingleOrDefault(t => t.WebSiteLanguageId == snippet.ContentSnippetLanguageId).PortalLanguageId);
                        currentPath = Helper.CreateFolder(currentPath, portalLanguage.LanguageCode);
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("Error Detected: " + page.Name);
                    }

                    var pathContent = Path.Combine(currentPath, $"{validName}-{snippet.ContentSnippetId}.html");

                    if (import)
                    {
                        try
                        {
                            var content = string.Empty;

                            if (File.Exists(pathContent))
                            {
                                content = File.ReadAllText(pathContent);
                                if (snippet.Value?.Length > 0)
                                    if (snippet.Value != content)
                                    {
                                        Console.WriteLine("Changes detected in content: " + snippet.Name);

                                        //Update a snippet
                                        JObject snippetObj = new JObject
                                            {
                                                { "adx_value", content }
                                            };

                                        var snippeturi = new Uri($"{svc.BaseAddress}adx_contentsnippets({snippet.ContentSnippetId})");
                                        svc.Patch(snippeturi, snippetObj);
                                        countUpdatedFiles++;
                                    }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (snippet.Value?.Length > 0)
                        {
                            File.WriteAllText(pathContent, snippet.Value);
                        }
                    }
                }
            }

            if (import)
            {
                Console.WriteLine($"-- Total Files Updated: {countUpdatedFiles} --");
            }
            else
            {
                Console.WriteLine($"-- Finished Downloading Files --");
            }

        }
    }
}
