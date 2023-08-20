using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio {
  public class AudioManager : ScriptableObject {
    [SerializeField] private FMODEventInstance _ambienceAudio;
    [SerializeField] private FMODParameter _ambienceSceneParam;

    private void OnEnable() {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        return;
      }
#endif
      if (_ambienceAudio != null) {
        _ambienceAudio.Setup();
      }
    }

    private void OnDisable() {
      if (_ambienceAudio.IsInitialized) {
        _ambienceAudio.Release();
      }
    }

    public void PlayAmbience() {
      if (_ambienceAudio.IsInitialized) {
        _ambienceAudio.SetParameter(_ambienceSceneParam, SceneManager.GetActiveScene().buildIndex);
        _ambienceAudio.Instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          _ambienceAudio.Play();
        }
      }
    }
  }
}
