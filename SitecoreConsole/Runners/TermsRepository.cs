using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SitecoreConsole.Runners
{
    public class TermsRepository
    {

        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _termsLookup =
            new ConcurrentDictionary<string, Dictionary<string, string>>();

        private readonly ConcurrentDictionary<string, Guid> _dbTermMap = new ConcurrentDictionary<string, Guid>();

        private readonly SqlConnection _longevityConnection;
        public TermsRepository()
        {
            _longevityConnection = GetDbConnection();
        }
        public void LoadTerms()
        {
            var terms = new Dictionary<Guid, string>();
            using (var connection = GetDbConnection())
            {
                var getTermQuery = "SELECT * FROM Terms;";
                var getTermCommand = new SqlCommand(getTermQuery, connection);
                var reader = getTermCommand.ExecuteReader();
                while (reader.Read())
                {
                    var termId = reader.GetGuid(0);
                    var term = reader.GetString(1);
                    terms[termId] = term;
                    _dbTermMap[term] = termId;
                    _termsLookup[term] = new Dictionary<string, string>();
                }
                reader.Close();
                var getTranslationQuery = "SELECT * FROM Translations;";
                var getTranslationCommand = new SqlCommand(getTranslationQuery, connection);
                var translationReader = getTranslationCommand.ExecuteReader();
                while (translationReader.Read())
                {
                    var termId = translationReader.GetGuid(1);
                    var lang = translationReader.GetString(2);
                    var value = translationReader.GetString(3);
                    if (!terms.ContainsKey(termId)) continue;
                    _termsLookup[terms[termId]][lang] = value;
                }
                translationReader.Close();
            }
            CustomConsoleLogger.Log($"Term Repository Initialized, found {_termsLookup.Count} terms", ConsoleColor.Green);
        }

        public void SaveTerms(string[] terms)
        {
            if (!terms.Any()) return;
            var formalizedTerms = terms.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLower().Replace("'", string.Empty)).ToArray();
            if (!formalizedTerms.Any()) return;
            var insertQuery = new StringBuilder("BEGIN TRANSACTION ");
            foreach (var term in formalizedTerms)
            {
                if (_dbTermMap.ContainsKey(term)) continue;
                var id = Guid.NewGuid();
                _dbTermMap[term] = id;
                insertQuery.AppendLine($"INSERT INTO Terms (Id, Term) VALUES ('{id}', N'{term}');");
            }
            insertQuery.AppendLine("COMMIT;");
            var cmd = new SqlCommand(insertQuery.ToString(), _longevityConnection);
            cmd.ExecuteNonQuery();
        }

        public void SaveTranslations(Dictionary<string, string> translations, string languageCode)
        {
            var insertQuery = new StringBuilder("BEGIN TRANSACTION ");
            foreach (var translation in translations)
            {
                var termId = _dbTermMap[translation.Key];
                var value = translation.Value;
                insertQuery.AppendLine($"INSERT INTO Translations (Id, TermId, LanguageCode, Translation) VALUES ('{Guid.NewGuid()}','{termId}','{languageCode}', N'{value}');");
            }
            insertQuery.AppendLine("COMMIT;");
            using (var connection = GetDbConnection())
            {
                var query = insertQuery.ToString();
                var command = new SqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            foreach (var translation in translations)
            {
                var term = translation.Key;
                var value = translation.Value;
                if (!_termsLookup.ContainsKey(term))
                    _termsLookup[term] = new Dictionary<string, string>();
                _termsLookup[term][languageCode] = value;
            }
            CustomConsoleLogger.Log($"Term Repository Saved {translations.Count} new terms", ConsoleColor.Cyan);
        }

        public string GetTranslation(string term, string language)
        {
            if (string.IsNullOrWhiteSpace(term)) return null;
            var formalizedTerm = term.Trim().ToLower().Replace("'", "");
            if (_termsLookup.ContainsKey(formalizedTerm) && _termsLookup[formalizedTerm].ContainsKey(language))
                return _termsLookup[formalizedTerm][language];
            return null;
        }

        public void ReadUnstranslatedTerms(string language, Action<string> onRead, Action onEnd)
        {
            var query =
                $"SELECT Term FROM Terms WHERE Id NOT IN (SELECT TermId FROM Translations WHERE LanguageCode = '{language}');";
            var cmd = new SqlCommand(query, _longevityConnection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var term = reader.GetString(0);
                if (_termsLookup.ContainsKey(term) && _termsLookup[term].ContainsKey(language)) continue;
                onRead(term);
            }
            reader.Close();
            onEnd();
        }

        private SqlConnection GetDbConnection()
        {
            var connString = ConfigurationManager.ConnectionStrings["translation"].ConnectionString;
            var connection = new SqlConnection(connString);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }
        
    }
}
