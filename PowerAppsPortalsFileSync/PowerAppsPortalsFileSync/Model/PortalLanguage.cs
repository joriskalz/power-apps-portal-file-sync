using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class PortalLanguage
    {
        [JsonProperty("adx_portallanguageid")]
        public Guid PortalLanguageId { get; set; }

        [JsonProperty("adx_languagecode")]
        public string LanguageCode { get; set; }
    }
}
