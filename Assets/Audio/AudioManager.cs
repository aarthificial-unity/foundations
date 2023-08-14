using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
  public class AudioManager : ScriptableObject {

    [SerializeField] private EventReference _ambienceEvent;
    private EventInstance _ambienceAudio;
    private PARAMETER_ID _ambienceSceneParameter;
    private bool _ambienceInitialized = false;

    private void Awake() {
      if (!_ambienceEvent.IsNull) {
        _ambienceAudio = RuntimeManager.CreateInstance(_ambienceEvent);
        RuntimeManager.StudioSystem.getParameterDescriptionByName("scene", out var parameter);
        _ambienceSceneParameter = parameter.id;
        _ambienceInitialized = true;
      }
    }

    public void PlayAmbience() {
      if (_ambienceInitialized) {
        RuntimeManager.StudioSystem.setParameterByID (
          _ambienceSceneParameter,
          SceneManager.GetActiveScene().buildIndex
        );
        _ambienceAudio.getPlaybackState (out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          _ambienceAudio.start();
        }
      }
    }

  }
}
