using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class WebSiteLanguage
    {
        [JsonProperty("adx_websitelanguageid")]
        public Guid WebSiteLanguageId { get; set; }

        [JsonProperty("_adx_websiteid_value")]
        public Guid WebSiteId { get; set; }

        [JsonProperty("_adx_portallanguageid_value")]
        public Guid PortalLanguageId { get; set; }

        [JsonProperty("adx_name")]
        public string Name { get; set; }
    }
}
