using Minio.Exceptions;
using Minio;
using SkiaSharp;

namespace ResizerASPNET.MinIO
{
    public class MinIOProvider : IMinIOProvider
    {
        private readonly MinioClient _minioClient;
        private readonly string _endpoint;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _nginxEndpoint;
        private readonly string _bucketName;

        public MinIOProvider()
        {
            string? endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT");
            string? accessKey = Environment.GetEnvironmentVariable("MINIO_ROOT_USER");
            string? secretKey = Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD");
            string? nginxEndpoint = Environment.GetEnvironmentVariable("MINIO_NGINX_ENDPOINT");
            string? bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");

            if (endpoint is null || accessKey is null 
                || secretKey is null || nginxEndpoint is null || bucketName is null)
            {
                throw new Exception("Environment not specified");
            }

            _endpoint = endpoint;
            _accessKey = accessKey;
            _secretKey = secretKey;
            _nginxEndpoint = nginxEndpoint;
            _bucketName = bucketName;
            _minioClient = new MinioClient()
                .WithEndpoint(_endpoint)
                .WithCredentials(_accessKey, _secretKey)
                .Build();
        }

        public void UploadFile(IFormFile file, string fileName)
        {
            var contentType = "binary/octet-stream";

            //stream.Position = 0;
            using var ms = new MemoryStream();
                file.CopyToAsync(ms).Wait();
            ms.Position = 0;

            try
            {
                var beArgs = new BucketExistsArgs()
                    .WithBucket(_bucketName);
                bool found = _minioClient.BucketExistsAsync(beArgs).Result;
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    _minioClient.MakeBucketAsync(mbArgs).Wait();
                }
                using (var filestream = ms)
                {
                    //Console.WriteLine("filestream.Position"+filestream.Position);
                    //Console.WriteLine(filestream.Length);
                    //Console.WriteLine(file.Length);
                    Console.WriteLine(_bucketName);
                    Console.WriteLine(fileName);
                    Console.WriteLine(contentType);
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(fileName)
                        .WithStreamData(filestream)
                        .WithObjectSize(filestream.Length)
                        .WithContentType(contentType);

                    Console.WriteLine("huetaaaaaaa" + putObjectArgs.ToString());
                    _minioClient.PutObjectAsync(putObjectArgs).Wait();
                }
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }

        public string GetDownloadLink(string filename)
        {
            var argss = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filename)
                .WithExpiry(60 * 60);
            string url = _minioClient.PresignedGetObjectAsync(argss).Result
                .Replace(_endpoint, _nginxEndpoint);
            return url;
        }

        public SKBitmap DownloadImage(string fileName, string downloadPath)
        {

            var getObjArgs = new GetObjectArgs()
                                    .WithBucket(_bucketName)
                                    .WithObject(fileName)
                                    .WithFile(downloadPath);

            var img = _minioClient.GetObjectAsync(getObjArgs).Result;

            var result = SKBitmap.Decode(downloadPath);
            return result;
        }
    }
}
