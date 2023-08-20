using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
  public class AudioManager : ScriptableObject {

    [SerializeField] public FMODEventInstance AmbienceAudio;
    private const string _ambienceSceneParam = "scene";

    private void Awake() {
      if (AmbienceAudio != null) {
        AmbienceAudio.Setup();
      }
    }

    public void PlayAmbience() {
      /*if (AmbienceAudio.IsInitialized) {
        AmbienceAudio.SetParameter(_ambienceSceneParam, SceneManager.GetActiveScene().buildIndex);
        AmbienceAudio.Instance.getPlaybackState (out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          AmbienceAudio.Play();
        }
      }*/
    }

  }
}
