using System;
using SitecoreConsole.Models;
using SitecoreConsole.Runners;

namespace SitecoreConsole
{
    class Program
    {
        private const string RootPath = "/sitecore/content/<your_site>/<Home>";
        private const string TargetLanguage = "zh-CN";
        private const string SitecoreTargetLanguage = "zh-HK";
        private static readonly string[] FieldTypes = { "Single-Line Text", "Multi-Line Text" };
        static void Main(string[] args)
        {
            var option = 'X';
            while (option != 'q')
            {
                option = ShowMenu();
                switch (option)
                {
                    case '1':
                        SaveAllTerms();
                        break;
                    case '2':
                        TranslateAllTerms();
                        break;
                    case '3':
                        UpdateTermsToSitecore();
                        break;
                    default:
                        break;
                }
            }
        }

        static char ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("1. Save All Terms");
            Console.WriteLine("2. Translate All Terms");
            Console.WriteLine("3. Update terms back to Sitecore");
            Console.WriteLine("Q. Quit");
            return Console.ReadKey().KeyChar;
        }

        private static void SaveAllTerms()
        {
            var repository = new TermsRepository();
            repository.LoadTerms();
            var contentReader = new ContentReader(repository);
            contentReader.Start(new ScanningOptions(RootPath, TargetLanguage, FieldTypes));
        }

        private static void TranslateAllTerms()
        {
            TermTranslator termTranslator = null;
            try
            {
                var repository = new TermsRepository();
                repository.LoadTerms();
                termTranslator = new TermTranslator();
                termTranslator.Start();
                var termCoordinator = new TermCoordinator(repository, termTranslator);
                termCoordinator.Start(TargetLanguage);
            }
            finally
            {
                termTranslator?.Stop();
            }
        }

        private static void UpdateTermsToSitecore()
        {
            var repository = new TermsRepository();
            repository.LoadTerms();
            var contentWriter = new ContentWriter(repository);
            contentWriter.Start(new WriteScanningOptions(RootPath, TargetLanguage, SitecoreTargetLanguage, FieldTypes));
        }
    }
}
