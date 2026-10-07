using UnityEngine;

namespace Modules.GameplayModule.Entities
{
    // The hideout's cassette player: its reels turn and its light is on while a tape plays.
    public class TapeDeckView : MonoBehaviour
    {
        [SerializeField] private Transform[] _reels = new Transform[0];
        [SerializeField] private GameObject _playingLight;
        [SerializeField] private float _reelSpeed = 220f;

        public void Apply(bool playing)
        {
            if (_playingLight != null && _playingLight.activeSelf != playing) _playingLight.SetActive(playing);
            if (!playing) return;
            foreach (Transform reel in _reels)
                if (reel != null) reel.Rotate(0f, 0f, -_reelSpeed * Time.deltaTime);
        }
    }
}
