using Settings.Bundles;
using UnityEngine;
using Utils;
using View.Controls;

namespace View.Settings {
  [DefaultExecutionOrder(100)]
  public class AudioSettingsView : MonoBehaviour {
    [Inject] [SerializeField] private AudioSettingsBundle _bundle;

    [SerializeField] private PaperSlider _masterVolume;
    [SerializeField] private PaperSlider _musicVolume;
    [SerializeField] private PaperSlider _sfxVolume;

    private void OnEnable() {
      _masterVolume.Value = _bundle.MasterVolume.Get();
      _musicVolume.Value = _bundle.MusicVolume.Get();
      _sfxVolume.Value = _bundle.SfxVolume.Get();

      _masterVolume.ValueChanged += _bundle.MasterVolume.Set;
      _musicVolume.ValueChanged += _bundle.MusicVolume.Set;
      _sfxVolume.ValueChanged += _bundle.SfxVolume.Set;
    }

    private void OnDisable() {
      _masterVolume.ValueChanged -= _bundle.MasterVolume.Set;
      _musicVolume.ValueChanged -= _bundle.MusicVolume.Set;
      _sfxVolume.ValueChanged -= _bundle.SfxVolume.Set;
    }
  }
}
