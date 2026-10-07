using System;

namespace Modules.AudioSettingsModule.Services
{
    // The player's volumes, each 0 to 1, kept between sessions. Master is applied to everything that sounds;
    // Music is a share the music player multiplies its own volume by.
    public interface IAudioSettingsService
    {
        float Master { get; }
        float Music { get; }
        void SetMaster(float value);
        void SetMusic(float value);
        event Action Changed;
    }
}
