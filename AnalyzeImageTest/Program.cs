using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;

namespace AnalyzeImageTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var application = new Application();
            application.ExecuteAsync().GetAwaiter().GetResult();

            Console.Read();
        }
    }

    class Application
    {
        public async Task ExecuteAsync()
        {
            var keyName = "TestIMage.png";
            var client = new AmazonRekognitionClient(RegionEndpoint.USEast1);
            var labelsRequest = new DetectLabelsRequest
            {
                Image = new Image
                {
                    S3Object = new S3Object()
                    {
                        Name = keyName,
                        Bucket = "rawimagestc1983"
                    }
                },
                MaxLabels = 10,
                MinConfidence = 75f
            };

            var moderationRequest = new DetectModerationLabelsRequest
            {
                Image = new Image
                {
                    S3Object = new S3Object()
                    {
                        Name = keyName,
                        Bucket = "rawimagestc1983"
                    }
                },
                MinConfidence = 60f
            };

            var labelsResponse = await client.DetectLabelsAsync(labelsRequest);
            var inappropriateResponse = await client.DetectModerationLabelsAsync(moderationRequest);
            Console.WriteLine("Detected Labels for Image");
            Console.WriteLine();

            foreach (Label label in labelsResponse.Labels)
                Console.WriteLine("{0}: {1}", label.Name, label.Confidence);
            Console.WriteLine();

            foreach (ModerationLabel label in inappropriateResponse.ModerationLabels)
                Console.WriteLine("Label: {0}\n Confidence: {1}\n Parent: {2}",
                    label.Name, label.Confidence, label.ParentName);
        }
    }
}
