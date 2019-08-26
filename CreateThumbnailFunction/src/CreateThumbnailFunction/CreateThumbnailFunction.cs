using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Functions
{
    public class CreateThumbnailFunction
    {
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(S3Event evnt, ILambdaContext context)
        {
            try
            {
                context.Logger.Log("Invoking Function");

                using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
                {
                    var s3Event = evnt.Records?[0].S3;
                    using (var imageStream = await GetS3Object(client, s3Event.Bucket.Name, s3Event.Object.Key))
                    {
                        context.Logger.LogLine("Got object - getting thumbnail stream");
                        using (var thumbnailImageStream = CreateThumbnailStream(imageStream))
                        {
                            context.Logger.LogLine("thumbnail stream created - saving to thumbnail bucket");
                            await PutThumbnailImage(client, s3Event.Object.Key, thumbnailImageStream);

                            context.Logger.LogLine("operation complete");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine(ex.Message);
                context.Logger.LogLine(ex.StackTrace);
            }
        }

        async Task<Stream> GetS3Object(AmazonS3Client client, string bucketName, string fileName)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using (var getResponse = await client.GetObjectAsync(getRequest))
            {
                return getResponse.ResponseStream;
            }
        }

        Stream CreateThumbnailStream(Stream rawImageStream)
        {
            using (var streamReader = new StreamReader(rawImageStream))
            {
                var streamBuffer = new byte[rawImageStream.Length];
                using (var memoryStream = new MemoryStream(streamBuffer))
                {
                    var image = SixLabors.ImageSharp.Image.Load(memoryStream.ToArray());
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size
                        {
                            Width = Convert.ToInt32(image.Width * 0.2m),
                            Height = Convert.ToInt32(image.Height * 0.2m)
                        },
                        Mode = ResizeMode.Stretch
                    };

                    image.Mutate(x => x.Resize(resizeOptions));
                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                        return outputStream;
                    }
                }
            }
        }

        async Task<bool> PutThumbnailImage(AmazonS3Client client, string keyName, Stream thumbnailImageStream)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = "thumbimagestc1983",
                Key = keyName,
                InputStream = thumbnailImageStream
            };

            await client.PutObjectAsync(putRequest);
            return true;
        }
    }
}
