using System;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalizationModule.Enums;
using Modules.LocalizationModule.Models;

namespace Modules.LocalizationModule.Services
{
    public class LocalizationService : ILocalizationService
    {
        [Inject] private ILocalizationModel _model { get; set; }

        public event Action LanguageChanged;
        public Language Language => _model.Language;

        public string Get(string key) => _model.Text(key);

        public string Format(string key, params object[] values)
        {
            string text = _model.Text(key);
            try { return string.Format(text, values); }
            catch (FormatException) { return text; }
        }

        public void SetLanguage(Language language)
        {
            if (language == _model.Language) return;
            _model.SetLanguage(language);
            LanguageChanged?.Invoke();
        }

        public void NextLanguage()
        {
            int count = Enum.GetValues(typeof(Language)).Length;
            SetLanguage((Language) (((int) _model.Language + 1) % count));
        }
    }
}
