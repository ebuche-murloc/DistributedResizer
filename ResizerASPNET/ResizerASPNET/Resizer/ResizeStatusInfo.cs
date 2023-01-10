using Newtonsoft.Json;

namespace ResizerASPNET.Resizer
{
    public class ResizeStatusInfo
    {
        [JsonProperty("resized")]
        public bool Resized { get; set; }
        [JsonProperty("miniolink")]
        public string MinioLink { get; set; }
        [JsonProperty("error")]
        public bool Error { get; set; }

        public ResizeStatusInfo(bool resized, string minioLink, bool error)
        {
            Resized = resized;
            MinioLink = minioLink;
            Error = error;
        }
    }
}
