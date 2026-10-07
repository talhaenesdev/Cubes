using System;
using Modules.LocalizationModule.Enums;

namespace Modules.LocalizationModule.Services
{
    // The one type other modules reference: the text for a key in the player's language, and the language itself.
    public interface ILocalizationService
    {
        Language Language { get; }
        // The text for a key; the key itself when no row in CD_Localization lists it, so plain text passes through.
        string Get(string key);
        // As Get, with {0}, {1}... filled in.
        string Format(string key, params object[] values);
        void SetLanguage(Language language);
        // The language after the current one, round the list: what a single toggle button switches to.
        void NextLanguage();
        // Raised after the language changed, so whatever shows text can show it again.
        event Action LanguageChanged;
    }
}
