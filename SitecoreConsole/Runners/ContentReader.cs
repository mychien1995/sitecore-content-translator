using System;
using System.Linq;
using System.Threading;
using SitecoreConsole.Models;
using Sitecore.Data.Items;

namespace SitecoreConsole.Runners
{
    public class ContentReader
    {
        private readonly TermsRepository _termsRepository;

        public ContentReader(TermsRepository termsRepository)
        {
            _termsRepository = termsRepository;
        }

        public void Start(ScanningOptions options)
        {
            CustomConsoleLogger.Log("Content Reader Started", ConsoleColor.Green);
            DoScan(options);
        }

        private void DoScan(ScanningOptions options)
        {
            var masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
            ScanItem(options.RootPath);
            void ScanItem(string path)
            {
                var rootContent = masterDb.GetItem(path);
                if (rootContent == null) return;
                rootContent.Fields.ReadAll();
                var includedFields = rootContent.Fields.Where(f =>
                    options.ScanFieldTypes.Contains(f.Type) && !string.IsNullOrEmpty(f.Type) && !f.Name.StartsWith("__")).ToArray();
                CustomConsoleLogger.Log($"Reading content {rootContent.Paths.FullPath} found {includedFields.Length} fields", ConsoleColor.DarkMagenta);
                var terms = includedFields.Select(f => f.Value).ToArray();
                _termsRepository.SaveTerms(terms);
                foreach (Item child in rootContent.Children)
                {
                    ScanItem(child.Paths.FullPath);
                }
            }
            Console.Write("Done");
        }
    }
}
