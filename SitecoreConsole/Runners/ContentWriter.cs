using System;
using System.Linq;
using Sitecore.Data.Items;
using SitecoreConsole.Models;
using Sitecore.Globalization;
using Sitecore.SecurityModel;

namespace SitecoreConsole.Runners
{
    public class ContentWriter
    {
        private readonly TermsRepository _termsRepository;

        public ContentWriter(TermsRepository termsRepository)
        {
            _termsRepository = termsRepository;
        }

        public void Start(WriteScanningOptions options)
        {
            CustomConsoleLogger.Log("Content Writer Started", ConsoleColor.Green);
            DoScan(options);
        }

        private void DoScan(WriteScanningOptions options)
        {
            var language = options.TargetLanguageCode;
            var masterDb = Sitecore.Configuration.Factory.GetDatabase("master");
            using (var disabler = new SecurityDisabler())
            {
                ScanItem(options.RootPath);
            }
            void ScanItem(string path)
            {
                var rootContent = masterDb.GetItem(path, Language.Parse(options.SitecoreLanguageCode));
                var enContent = masterDb.GetItem(path);
                if (enContent == null || rootContent == null)
                {
                    CustomConsoleLogger.Log($"{path} must have a language version of {language}", ConsoleColor.Red);
                    return;
                }
                rootContent.Fields.ReadAll();
                enContent.Fields.ReadAll();
                var includedFields = enContent.Fields.Where(f =>
                    options.ScanFieldTypes.Contains(f.Type) && !string.IsNullOrEmpty(f.Type) &&
                    !f.Name.StartsWith("__")).ToArray();
                if (includedFields.Any())
                {
                    var fieldCount = 0;
                    rootContent.Editing.BeginEdit();
                    foreach (var enField in includedFields)
                    {
                        var term = enField.Value;
                        var translation = _termsRepository.GetTranslation(term, language);
                        if (translation != null)
                        {
                            var matchingField = rootContent.Fields.FirstOrDefault(f => f.ID == enField.ID);
                            if (matchingField != null)
                            {
                                fieldCount++;
                                matchingField.Value = translation;
                            }
                        }
                    }

                    rootContent.Editing.EndEdit();
                    if (fieldCount > 0)
                        CustomConsoleLogger.Log(
                            $"Updated content {rootContent.Paths.FullPath} found {fieldCount} fields",
                            ConsoleColor.DarkMagenta);
                }
                foreach (Item child in rootContent.Children)
                {
                    ScanItem(child.Paths.FullPath);
                }
            }
            Console.Write("Done");
        }
    }
}
