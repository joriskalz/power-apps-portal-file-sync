using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class WebSite
    {
        [JsonProperty("adx_websiteid")]
        public Guid WebSiteId { get; set; }

        [JsonProperty("adx_name")]
        public string Name { get; set; }
    }
}
