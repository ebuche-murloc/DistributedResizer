using Newtonsoft.Json;

namespace ResizerAPI.Rabbit
{
    public class ImageInfo
    {
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("filename")]
        public string Filename { get; set; }

        public ImageInfo(int height, int width, string filename)
        {
            Height = height;
            Width = width;
            Filename = filename;
        }
    }
}
