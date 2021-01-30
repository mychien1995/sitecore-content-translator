namespace SitecoreConsole.Models
{
    public class TranslationRequest
    {
        public TranslationRequest(string term, string languageCode)
        {
            LanguageCode = languageCode;
            Term = term.Trim().ToLower();
        }

        public string Term { get;  }
        public string LanguageCode { get; }
    }
}
