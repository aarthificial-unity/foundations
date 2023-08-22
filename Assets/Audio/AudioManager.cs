using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
  public class AudioManager : ScriptableObject {
    [SerializeField] private FMODEventInstance _ambienceAudio;
    [SerializeField] private FMODParameterInstance _ambienceSceneParam;

    private void OnEnable() {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        return;
      }
#endif
      _ambienceAudio.Setup();
      _ambienceSceneParam.Setup();
    }

    private void OnDisable() {
      _ambienceAudio.Release();
    }

    public void PlayAmbience() {
      if (_ambienceAudio.IsInitialized) {
        _ambienceSceneParam.CurrentValue = SceneManager.GetActiveScene().buildIndex;
        _ambienceAudio.Instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          _ambienceAudio.Play();
        }
      }
    }
  }
}
