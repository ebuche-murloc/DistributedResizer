using Newtonsoft.Json;
using System.Web;
using static System.Net.WebRequestMethods;

namespace ResizerASPNET.Resizer
{
    public class ResizerAPI : IResizerAPI
    {
        private readonly HttpClient _http = new();
        private readonly string _apiEndpoint;

        public ResizerAPI() 
        {
            string? apiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
            if (apiEndpoint is null )
            {
                throw new Exception("Environment not specified");
            }
            _apiEndpoint = apiEndpoint;
        }

        public ResizeStatusInfo Resize(int height, int width, string file)
        {
            var builder = new UriBuilder($"http://{_apiEndpoint}/Home/Resize");
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["height"] = height.ToString();
            query["width"] = width.ToString();
            query["file"] = file;
            builder.Query = query.ToString();
            string httpRequestString = builder.ToString();
            string text = GetResponseTextWithHttp(httpRequestString, TimeSpan.FromSeconds(60));
            ResizeStatusInfo? result = TryDeserialize(text);
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
