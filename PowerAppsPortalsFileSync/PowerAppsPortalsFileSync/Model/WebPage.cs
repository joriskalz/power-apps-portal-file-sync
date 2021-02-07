using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class WebPage
    {
        [JsonProperty("adx_webpageid")]
        public Guid WebPageId { get; set; }

        [JsonProperty("adx_name")]
        public string Name { get; set; }

        [JsonProperty("adx_partialurl")]
        public string adx_partialurl { get; set; }

        [JsonProperty("adx_isroot")]
        public bool IsRoot { get; set; }

        [JsonProperty("_adx_webpagelanguageid_value")]
        public Guid? WebPageLanguageId { get; set; }

        [JsonProperty("_adx_parentpageid_value")]
        public string ParentPageId { get; set; }

        [JsonProperty("_adx_websiteid_value")]
        public string WebsiteId { get; set; }

        [JsonProperty("adx_copy")]
        public string Copy { get; set; }

        [JsonProperty("adx_customcss")]
        public string CustomCss { get; set; }

        [JsonProperty("adx_customjavascript")]
        public string CustomJavascript { get; set; }

    }
}
