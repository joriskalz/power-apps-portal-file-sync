using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class WebTemplate
    {
        [JsonProperty("adx_webtemplateid")]
        public Guid WebTemplateId { get; set; }

        [JsonProperty("adx_name")]
        public string Name { get; set; }

        [JsonProperty("adx_source")]
        public string Source { get; set; }

    }
}
