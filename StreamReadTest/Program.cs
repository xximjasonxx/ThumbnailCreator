using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace StreamReadTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var application = new Application();
            application.Execute().GetAwaiter().GetResult();

            Console.Read();
        }
    }

    class Application
    {
        async Task<bool> WriteS3Object(string bucketName, string keyName, Stream contentStream)
        {
            using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = contentStream
                };

                await client.PutObjectAsync(request);
                return true;
            }
        }

        async Task<GetObjectResponse> GetS3Object(string bucketName, string keyName)
        {
            using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                ResponseHeaderOverrides responseHeaders = new ResponseHeaderOverrides();
                responseHeaders.CacheControl = "No-cache";
                request.ResponseHeaderOverrides = responseHeaders;

                return await client.GetObjectAsync(request);
            }
        }

        Stream GetResizedStream(Stream stream, decimal scalingFactor, string mimeType)
        {
            using (Image<Rgba32> image = Image.Load(stream))
            {
                var resizeOptions = new ResizeOptions
                {
                    Size = new SixLabors.Primitives.Size
                    {
                        Width = Convert.ToInt32(image.Width * scalingFactor),
                        Height = Convert.ToInt32(image.Height * scalingFactor)
                    },
                    Mode = ResizeMode.Stretch
                };

                image.Mutate(x => x.Resize(resizeOptions));

                var memoryStream = new MemoryStream();
                image.Save(memoryStream, mimeType.AsEncoder());

                return memoryStream;
            }
        }

        public async Task Execute()
        {
            var getResponse = await GetS3Object("rawimagestc1983", "Test1");
            using (var responseStream = getResponse.ResponseStream)
            {
                using (var resizedStream = GetResizedStream(responseStream, 0.5m, getResponse.Headers.ContentType))
                {
                    resizedStream.Seek(0, SeekOrigin.Begin);
                    await WriteS3Object("thumbimagestc1983", "thumb_Test1", resizedStream);
                }
            }

            Console.WriteLine("Complete");
        }
    }
}