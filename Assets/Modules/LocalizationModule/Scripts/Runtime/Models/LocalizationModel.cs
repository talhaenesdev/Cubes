using System.Collections.Generic;
using FlowIoC.BaseModule.Adapters;
using FlowIoC.BaseModule.Constructables;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Data.ValueObjects;
using Modules.LocalizationModule.Enums;
using Modules.LocalizationModule.RootsContexts;
using UnityEngine;

namespace Modules.LocalizationModule.Models
{
    // Reads CD_Localization off the Root's adapter; keeps the chosen language in PlayerPrefs, starting from the
    // system's language the first time.
    public class LocalizationModel : ILocalizationModel, IConstructable
    {
        private const string LanguageKey = "localization.language";
        [Inject(nameof(LocalizationServiceContext))] private GameObject _root { get; set; }
        private readonly Dictionary<string, LocalizedTextCVO> _texts = new();

        public bool IsPostConstructed { get; set; }
        public bool IsDeconstructed { get; set; }
        public Language Language { get; private set; }

        public void PostConstruct()
        {
            var adapter = _root != null ? _root.GetComponent<RootAdapter>() : null;
            CD_Localization table = adapter != null ? adapter.GetScriptable<CD_Localization>() : null;
            if (table == null) FlowLogger.LogError("LocalizationServiceRoot files no CD_Localization; every text is shown by its key.", _root);
            else
                foreach (LocalizedTextCVO text in table.Texts)
                    if (!string.IsNullOrEmpty(text.Key)) _texts[text.Key] = text;
            Language fallback = Application.systemLanguage == SystemLanguage.Turkish ? Language.Turkish : Language.English;
            Language = (Language) PlayerPrefs.GetInt(LanguageKey, (int) fallback);
        }

        public void Deconstruct() => _texts.Clear();

        public void SetLanguage(Language language)
        {
            Language = language;
            PlayerPrefs.SetInt(LanguageKey, (int) language);
            PlayerPrefs.Save();
        }

        public string Text(string key)
        {
            if (string.IsNullOrEmpty(key) || !_texts.TryGetValue(key, out LocalizedTextCVO text)) return key ?? string.Empty;
            // A row may leave English empty when its key already is the English text.
            string found = Language == Language.Turkish ? text.Turkish : text.English;
            if (string.IsNullOrEmpty(found)) found = string.IsNullOrEmpty(text.English) ? key : text.English;
            return found;
        }
    }
}
