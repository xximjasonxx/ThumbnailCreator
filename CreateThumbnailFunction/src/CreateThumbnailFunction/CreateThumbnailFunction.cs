using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;

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

                var s3Event = evnt.Records?[0].S3;
                var getRequest = new GetObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = s3Event.Object.Key
                };

                using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
                {
                    using (var getResponse = await client.GetObjectAsync(getRequest))
                    {
                        using (var rawImage = Image.FromStream(getResponse.ResponseStream))
                        {
                            context.Logger.LogLine("Got image - creating thumbnail");
                            var thumbnail = rawImage.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
                            thumbnail.Save("thumbnail");
                            context.Logger.LogLine("Save Image to file");

                            // put the thumbnail
                            var putRequest = new PutObjectRequest
                            {
                                BucketName = "thumbimagestc1983",
                                Key = s3Event.Object.Key,
                                FilePath = "thumbnail"
                            };

                            var putResponse = await client.PutObjectAsync(putRequest);
                            context.Logger.LogLine("Write Complete");
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
    }
}
