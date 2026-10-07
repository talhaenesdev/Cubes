using Modules.LocalizationModule.Enums;

namespace Modules.LocalizationModule.Models
{
    public interface ILocalizationModel
    {
        Language Language { get; }
        void SetLanguage(Language language);
        // The text for a key in the current language; the key itself when no row lists it.
        string Text(string key);
    }
}
