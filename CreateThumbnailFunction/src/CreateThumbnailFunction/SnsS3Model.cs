
using System.Collections.Generic;

namespace CreateThumbnailFunction
{
    public class SnsRecord
    {
        public ICollection<SnsS3Model> Records { get; set; }
    }

    public class SnsS3Model
    {
        public S3Model S3 { get; set; }
    }

    public class S3Model
    {
        public S3BucketModel Bucket { get; set; }
        public S3ObjectModel Object { get; set; }
    }

    public class S3BucketModel
    {
        public string Name { get; set; }
    }

    public class S3ObjectModel
    {
        public string Key { get; set; }
    }
}