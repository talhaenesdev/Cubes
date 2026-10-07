using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Enums;
using UnityEngine;

namespace Modules.GameplayModule.Data.UnityObjects
{
    // Every piece of music. The hideout plays a Hideout track and a contract a Contract track - its mission's own
    // when it names one, else HideoutTrack / ContractTrack when set, else one picked at random from the category.
    [CreateAssetMenu(fileName = "CD_Music", menuName = "Night Shift/CD_Music")]
    public class CD_Music : ScriptableObject
    {
        [Tooltip("Always this in the hideout. Empty picks a random Hideout track each visit.")]
        public string HideoutTrack;
        [Tooltip("Always this on a contract whose mission names none. Empty picks a random Contract track.")]
        public string ContractTrack;
        public List<MusicTrackCVO> Tracks = new();

        public MusicTrackCVO Pick(MusicCategory category)
        {
            var pool = Tracks.FindAll(track => track.Category == category && track.Clip != null);
            return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
        }

        public MusicTrackCVO Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (MusicTrackCVO track in Tracks)
                if (track.Id == id) return track;
            return null;
        }
    }
}
