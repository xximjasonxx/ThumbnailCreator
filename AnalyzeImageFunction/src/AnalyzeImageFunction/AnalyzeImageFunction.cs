
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using AnalyzeImageFunction;
using Newtonsoft.Json;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AnalyzeImageFunction;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Functions
{
    public class AnalyzeImageFunction
    {

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ExecuteAsync(SNSEvent evnt, ILambdaContext context)
        {
            var snsData = JsonConvert.DeserializeObject<SnsRecord>(evnt.Records?[0].Sns.Message);
            var s3Data = snsData.Records.ElementAt(0).S3;
            using (var client = new AmazonRekognitionClient(RegionEndpoint.USEast1))
            {
                var detectedLabels = await DetectLabelsAsync(client,
                    s3Data.Bucket.Name,
                    s3Data.Object.Key);

                foreach (Label label in detectedLabels)
                    context.Logger.LogLine($"{label.Name}: {label.Confidence}");
            }
        }

        async Task<IList<Label>> DetectLabelsAsync(AmazonRekognitionClient client, string bucketName, string keyName)
        {
            var labelsRequest = new DetectLabelsRequest
            {
                Image = new Image
                {
                    S3Object = new S3Object()
                    {
                        Name = keyName,
                        Bucket = bucketName
                    }
                },
                MaxLabels = 10,
                MinConfidence = 75f
            };

            var response = await client.DetectLabelsAsync(labelsRequest);
            return response.Labels;
        }

        async Task WriteToDynamoTableAsync(string imageName, ICollection<Label> labelList)
        {
            var clientConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1
            };

            using (var client = new AmazonDynamoDBClient(clientConfig))
            {
                var request = new PutItemRequest
                {
                    TableName = "ImageDataTable",
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "ImageName", new AttributeValue { S = imageName } },
                        { "Data", new AttributeValue { S = new JArray(labelList.Select((label) => new JObject(new JProperty(label.Name, label.Confidence)))).ToString() } }
                    }
                };

                await client.PutItemAsync(request);
            }
        }
    }
}
