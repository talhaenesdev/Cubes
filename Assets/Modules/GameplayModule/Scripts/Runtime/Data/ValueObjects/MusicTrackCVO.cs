using System;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.ValueObjects
{
    // One piece of music. Dropped into Art/Audio/Music it is listed here on import; fill in the credit by hand.
    [Serializable]
    public class MusicTrackCVO
    {
        [Tooltip("The key missions name this track by.")]
        public string Id = "new_track";
        public string Title = "UNTITLED";
        public string Artist = "UNKNOWN ARTIST";
        [Tooltip("Hideout tracks play at home, Contract tracks on jobs. Set from the folder the file is in.")]
        public MusicCategory Category = MusicCategory.Contract;
        [Tooltip("The attribution the licence asks for, shown in credits. Kevin MacLeod's CC BY tracks need it.")]
        [TextArea(2, 4)] public string Credit = "";
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 0.7f;
    }
}
