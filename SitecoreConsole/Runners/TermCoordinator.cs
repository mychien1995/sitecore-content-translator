using System;
using System.Collections.Generic;
using System.Linq;

namespace SitecoreConsole.Runners
{
    public class TermCoordinator
    {
        private readonly TermsRepository _termsRepository;
        private readonly TermTranslator _termTranslator;
        private const int RequestThreshold = 10000;

        public TermCoordinator(TermsRepository termsRepository, TermTranslator termTranslator)
        {
            _termsRepository = termsRepository;
            _termTranslator = termTranslator;
        }

        public void Start(string languageCode)
        {
            CustomConsoleLogger.Log("Term Coordinator Started", ConsoleColor.Green);
            var batch = new List<string>();
            _termsRepository.ReadUnstranslatedTerms(languageCode, (term) =>
            {
                batch.Add(term);
                if (batch.Count != 100) return;
                ProcessBatch();
            }, ProcessBatch);

            void ProcessBatch()
            {
                if (!batch.Any()) return;
                var result = _termTranslator.Translate(batch, languageCode);
                _termsRepository.SaveTranslations(result, languageCode);
                batch = new List<string>();
            }
        }
    }
}
