using ResizerASPNET.Rabbit;
using Newtonsoft.Json;
using System.Web;
using static System.Net.WebRequestMethods;

namespace ResizerASPNET.Resizer
{
    public class ResizerAPI : IResizerAPI
    {
        private readonly IRpcClient _rpcRabbit;
        private readonly HttpClient _http = new();
        private readonly string _apiEndpoint;

        public ResizerAPI(IRpcClient rpcRabbit) 
        {
            string? apiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
            if (apiEndpoint is null )
            {
                throw new Exception("Environment not specified");
            }
            _apiEndpoint = apiEndpoint;
            _rpcRabbit = rpcRabbit;
        }

        public ResizeStatusInfo Resize(int height, int width, string file)
        {
            var imageInfo = new ImageInfo(height, width, file);
            string json = JsonConvert.SerializeObject(imageInfo);
            var response = _rpcRabbit.SendAsync(json).Result;
            ResizeStatusInfo? result = TryDeserialize(response);
            return result is null ? new ResizeStatusInfo(false, "", true) : result;
        }

        private ResizeStatusInfo? TryDeserialize(string text) 
        {
            try
            {
                return JsonConvert.DeserializeObject<ResizeStatusInfo>(text);
            }
            catch { }
            return null;
        }

        private string GetResponseTextWithHttp(string request, TimeSpan timeout)
        {
            try
            {
                Task<HttpResponseMessage> httpRequest;
                httpRequest = _http.GetAsync(request);
                if (!httpRequest.Wait(timeout)
                || httpRequest.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return "";
                }
                var httpResponse = httpRequest.Result;
                return httpResponse.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return "";
            }
        }
    }
}
