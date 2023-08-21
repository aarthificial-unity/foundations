using Settings.Bundles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.Controls;

namespace View.Settings {
  public class VideoSettingsView : MonoBehaviour {
    [Inject] [SerializeField] private VideoSettingsBundle _bundle;

    [SerializeField] private TMP_Dropdown _displayMode;
    [SerializeField] private TMP_Dropdown _resolution;
    [SerializeField] private TMP_Dropdown _quality;
    [SerializeField] private PaperSlider _brightness;
    [SerializeField] private Toggle _vsync;
    [SerializeField] private PaperSlider _targetFramerate;

    private void OnEnable() {
      _resolution.options.Clear();
      foreach (var key in _bundle.ResolutionKeys) {
        _resolution.options.Add(new TMP_Dropdown.OptionData(key));
      }

      _displayMode.value = _bundle.DisplayMode.Get();
      _resolution.value = _bundle.Resolution.Get();
      _quality.value = _bundle.Quality.Get();
      _brightness.Value = _bundle.Brightness.Get();
      _vsync.isOn = _bundle.Vsync.GetBool();
      _targetFramerate.Value = _bundle.TargetFramerate.Get();

      _displayMode.onValueChanged.AddListener(HandleDisplayModeChanged);
      _resolution.onValueChanged.AddListener(HandleResolutionChanged);
      _quality.onValueChanged.AddListener(HandleQualityChanged);
      _brightness.ValueChanged += HandleBrightnessChanged;
      _vsync.onValueChanged.AddListener(HandleVsyncChanged);
      _targetFramerate.ValueChanged += HandleTargetFramerateChanged;

      _bundle.Vsync.Changed += HandleVsyncSettingChanged;
    }

    private void OnDisable() {
      _displayMode.onValueChanged.RemoveListener(HandleDisplayModeChanged);
      _resolution.onValueChanged.RemoveListener(HandleResolutionChanged);
      _quality.onValueChanged.RemoveListener(HandleQualityChanged);
      _brightness.ValueChanged -= HandleBrightnessChanged;
      _vsync.onValueChanged.RemoveListener(HandleVsyncChanged);
      _targetFramerate.ValueChanged -= HandleTargetFramerateChanged;

      _bundle.Vsync.Changed -= HandleVsyncSettingChanged;
    }

    private void HandleDisplayModeChanged(int mode) {
      _bundle.DisplayMode.Set(mode);
    }

    private void HandleResolutionChanged(int resolution) {
      _bundle.Resolution.Set(resolution);
    }

    private void HandleQualityChanged(int quality) {
      _bundle.Quality.Set(quality);
    }

    private void HandleBrightnessChanged(int brightness) {
      _bundle.Brightness.Set(brightness);
    }

    private void HandleVsyncChanged(bool isEnabled) {
      _bundle.Vsync.Set(isEnabled);
    }

    private void HandleTargetFramerateChanged(int framerate) {
      _bundle.TargetFramerate.Set(framerate);
    }

    private void HandleVsyncSettingChanged(int value) {
      _targetFramerate.SetInteractive(value == 0);
    }
  }
}
