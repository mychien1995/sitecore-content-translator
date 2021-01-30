namespace SitecoreConsole.Models
{
    public class ScanningOptions
    {
        public string RootPath { get;  }
        public string TargetLanguageCode { get; }
        public string[] ScanFieldTypes { get;  }

        public ScanningOptions(string rootPath, string targetLanguageCode, string[] scanFieldTypes)
        {
            RootPath = rootPath;
            TargetLanguageCode = targetLanguageCode;
            ScanFieldTypes = scanFieldTypes;
        }
    }

    public class WriteScanningOptions : ScanningOptions
    {
        public WriteScanningOptions(string rootPath, string targetLanguageCode, string sitecoreLanguageCode, string[] scanFieldTypes)
            : base(rootPath, targetLanguageCode, scanFieldTypes)
        {
            SitecoreLanguageCode = sitecoreLanguageCode;
        }

        public string SitecoreLanguageCode { get; }
    }
}
