using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
  public class AudioManager : ScriptableObject {
    [SerializeField] private EventReference _ambienceEvent;
    private EventInstance _ambienceAudio;
    private PARAMETER_ID _ambienceSceneParameter;
    private bool _ambienceInitialized;

    private void OnEnable() {
      _ambienceInitialized = false;
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        return;
      }
#endif

      if (!_ambienceEvent.IsNull) {
        _ambienceAudio = RuntimeManager.CreateInstance(_ambienceEvent);
        _ambienceAudio.getDescription(out var description);
        description.getParameterDescriptionByName("scene", out var parameter);
        _ambienceSceneParameter = parameter.id;
        _ambienceInitialized = true;
      }
    }

    private void OnDisable() {
      if (_ambienceInitialized) {
        _ambienceAudio.release();
      }
    }

    public void PlayAmbience() {
      if (_ambienceInitialized) {
        _ambienceAudio.setParameterByID(
          _ambienceSceneParameter,
          SceneManager.GetActiveScene().buildIndex
        );
        _ambienceAudio.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          _ambienceAudio.start();
        }
      }
    }
  }
}
