using Audio.Events;
using Audio.Parameters;
using FMOD.Studio;
using FMODUnity;
using Settings.Bundles;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Audio {
  public class AudioManager : MonoBehaviour {
    [Inject] [SerializeField] private AudioSettingsBundle _bundle;
    [SerializeField] private FMODEventInstance _ambientSound;
    [SerializeField] private FMODParameterInstance _sceneParam;

    private VCA _masterVca;
    private VCA _musicVca;
    private VCA _sfxVca;

    private void OnEnable() {
      RuntimeManager.StudioSystem.getVCA("vca:/Master", out _masterVca);
      RuntimeManager.StudioSystem.getVCA("vca:/Music", out _musicVca);
      RuntimeManager.StudioSystem.getVCA("vca:/SFX", out _sfxVca);

      _bundle.MasterVolume.Changed += HandleMasterVolumeChanged;
      _bundle.MusicVolume.Changed += HandleMusicVolumeChanged;
      _bundle.SfxVolume.Changed += HandleSfxVolumeChanged;

      SceneManager.sceneLoaded += HandleSceneLoaded;

      _sceneParam.Setup();
      _sceneParam.CurrentValue = SceneManager.GetActiveScene().buildIndex;
      _ambientSound.Setup();
      _ambientSound.Play();
    }

    private void OnDisable() {
      _bundle.MasterVolume.Changed -= HandleMasterVolumeChanged;
      _bundle.MusicVolume.Changed -= HandleMusicVolumeChanged;
      _bundle.MusicVolume.Changed -= HandleMusicVolumeChanged;
      SceneManager.sceneLoaded -= HandleSceneLoaded;
      _ambientSound.Pause();
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

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode){
      _sceneParam.CurrentValue = scene.buildIndex;
    }
  }
}
