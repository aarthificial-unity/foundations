using Audio.Ambiance;
using FMOD.Studio;
using FMODUnity;
using Settings.Bundles;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Audio {
  public class AudioManager : MonoBehaviour {
    [Inject] [SerializeField] private AudioSettingsBundle _bundle;

    [SerializeField] private FMODEventInstance _ambienceAudio;
    [SerializeField] private FMODParameterInstance _ambienceSceneParam;

    private VCA _masterVca;
    private VCA _musicVca;
    private VCA _sfxVca;

    private void OnEnable() {
      _ambienceAudio.Setup();
      _ambienceSceneParam.Setup();

      RuntimeManager.StudioSystem.getVCA("vca:/Master", out _masterVca);
      RuntimeManager.StudioSystem.getVCA("vca:/Music", out _musicVca);
      RuntimeManager.StudioSystem.getVCA("vca:/SFX", out _sfxVca);

      _bundle.MasterVolume.Changed += HandleMasterVolumeChanged;
      _bundle.MusicVolume.Changed += HandleMusicVolumeChanged;
      _bundle.SfxVolume.Changed += HandleSfxVolumeChanged;
    }

    private void OnDisable() {
      _ambienceAudio.Release();

      _bundle.MasterVolume.Changed -= HandleMasterVolumeChanged;
      _bundle.MusicVolume.Changed -= HandleMusicVolumeChanged;
      _bundle.MusicVolume.Changed -= HandleMusicVolumeChanged;
    }

    public void SetAmbiance(AmbianceType ambianceType) {
      if (_ambienceAudio.IsInitialized) {
        _ambienceSceneParam.CurrentValue = ambianceType.Value;
        _ambienceAudio.Instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.STOPPED) {
          _ambienceAudio.Play();
        }
      }
    }

    private void HandleMasterVolumeChanged(int value) {
      _masterVca.setVolume(value / 100f);
    }

    private void HandleMusicVolumeChanged(int value) {
      _musicVca.setVolume(value / 100f);
    }

    private void HandleSfxVolumeChanged(int value) {
      _sfxVca.setVolume(value / 100f);
    }
  }
}
