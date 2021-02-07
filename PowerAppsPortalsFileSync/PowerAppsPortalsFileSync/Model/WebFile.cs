using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerAppsPortalsFileSync.Model
{
    public class WebFile
    {
        [JsonProperty("annotationid")]
        public Guid AnnotationId { get; set; }

        [JsonProperty("_objectid_value")]
        public Guid ObjectId { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("documentbody")]
        public string DocumentBody { get; set; }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        [JsonProperty("isdocument")]
        public bool IsDocument { get; set; }

    }
}
