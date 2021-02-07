using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class ContentSnippet
    {
        [JsonProperty("adx_contentsnippetid")]
        public Guid ContentSnippetId { get; set; }

        [JsonProperty("adx_name")]
        public string Name { get; set; }

        [JsonProperty("adx_value")]
        public string Value { get; set; }

        [JsonProperty("_adx_contentsnippetlanguageid_value")]
        public Guid? ContentSnippetLanguageId { get; set; }

    }
}
