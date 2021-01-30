using System;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;

namespace SitecoreConsole.Models
{
    public class ScanningOptions
    {
        public string RootPath { get; }
        public string TargetLanguageCode { get; }
        public string[] ScanFieldTypes { get; }

        public ScanningOptions(string rootPath, string targetLanguageCode, string[] scanFieldTypes)
        {
            RootPath = rootPath;
            TargetLanguageCode = targetLanguageCode;
            ScanFieldTypes = scanFieldTypes;
        }
    }

    public class WriteScanningOptions : ScanningOptions
    {
        public string[] ExcludedTemplates { get; set; } = Array.Empty<string>();
        public string[] ExcludedBaseTemplates { get; set; } = Array.Empty<string>();
        public WriteScanningOptions(string rootPath, string targetLanguageCode, string sitecoreLanguageCode, string[] scanFieldTypes)
            : base(rootPath, targetLanguageCode, scanFieldTypes)
        {
            SitecoreLanguageCode = sitecoreLanguageCode;
        }

        public bool ShouldWrite(Item item)
        {
            var templateId = item.TemplateID.ToString();
            if (ExcludedTemplates.Contains(templateId)) return false;
            if (ExcludedBaseTemplates.Any(t => IsDerived(item, ID.Parse(t)))) return false;
            return true;
        }

        static bool IsDerived(Item item, ID templateId)
        {
            var template = TemplateManager.GetTemplate(item);
            if (template == null)
            {
                return false;
            }
            return IsDerived(template, templateId);
        }
        static bool IsDerived(Template template, ID templateId)
        {
            return template.ID == templateId ||
                   template.GetBaseTemplates().Any(baseTemplate => IsDerived(baseTemplate, templateId));
        }
        public string SitecoreLanguageCode { get; }
    }
}
