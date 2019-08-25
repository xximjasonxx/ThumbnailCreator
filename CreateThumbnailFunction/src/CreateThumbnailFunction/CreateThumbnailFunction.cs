using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;

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
        public async Task<string> ExecuteAsync(S3Event evnt, ILambdaContext context)
        {
            var s3Event = evnt.Records?[0].S3;
            context.Logger.Log("Invoking Function");
            /*if (s3Event == null)
            {
                return "No Event";
            }

            return $"Event Received - {s3Event.Object.Key}";*/
            return "Executed";
        }
    }
}
