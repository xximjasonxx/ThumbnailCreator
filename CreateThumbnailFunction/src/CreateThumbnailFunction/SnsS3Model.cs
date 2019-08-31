
namespace CreateThumbnailFunction
{
    public class SnsS3Model
    {
        public string Bucket { get; set; }
        public S3ObjectModel Object { get; set; }
    }

    public class S3ObjectModel
    {
        public string Key { get; set; }
    }
}