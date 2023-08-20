using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Utils;

namespace Settings.Bundles {
  public class VideoSettingsBundle : ScriptableObject {
    public SettingProperty DisplayMode;
    public SettingProperty Resolution;
    public SettingProperty Quality;
    public SettingProperty Brightness;
    public SettingProperty Vsync;
    public SettingProperty TargetFramerate;

    public readonly List<string> ResolutionKeys = new();
    private readonly HashSet<string> _resolutionKeys = new();
    private readonly List<Resolution> _resolutions = new();

    [SerializeField] private VolumeProfile _globalVolumeProfile;

    private void RecreateResolutions() {
      _resolutions.Clear();
      _resolutionKeys.Clear();
      ResolutionKeys.Clear();

      for (var i = Screen.resolutions.Length - 1; i >= 0; i--) {
        var resolution = Screen.resolutions[i];
        var key = $"{resolution.width} x {resolution.height}";
        if (_resolutionKeys.Contains(key)) {
          continue;
        }

        ResolutionKeys.Add(key);
        _resolutionKeys.Add(key);
        _resolutions.Add(resolution);
      }
    }

    private void OnEnable() {
      RecreateResolutions();

      DisplayMode = new SettingProperty("displayMode");
      Resolution = new SettingProperty("resolution");
      Quality = new SettingProperty(
        "quality",
        QualitySettings.GetQualityLevel()
      );
      Brightness = new SettingProperty("brightness", 50);
      Vsync = new SettingProperty("vsync", 1);
      TargetFramerate = new SettingProperty("targetFramerate", 60);

      DisplayMode.Changed += HandleDisplayModeChanged;
      Resolution.Changed += HandleResolutionChanged;
      Quality.Changed += HandleQualityChanged;
      Brightness.Changed += HandleBrightnessChanged;
      Vsync.Changed += HandleVsyncChanged;
      TargetFramerate.Changed += HandleTargetFramerateChanged;
    }

    private void HandleDisplayModeChanged(int _) {
      UpdateScreen();
    }

    private void HandleResolutionChanged(int _) {
      UpdateScreen();
    }

    private void HandleQualityChanged(int quality) {
      QualitySettings.SetQualityLevel(quality);
    }

    private void HandleBrightnessChanged(int brightness) {
      if (_globalVolumeProfile.TryGet(out ColorAdjustments colors)) {
        colors.postExposure.overrideState = true;
        colors.postExposure.value =
          ((float)brightness).ClampRemap(0, 100, -3, 3);
      }
    }

    private void HandleVsyncChanged(int isEnabled) {
      QualitySettings.vSyncCount = isEnabled;
    }

    private void HandleTargetFramerateChanged(int framerate) {
      Application.targetFrameRate = framerate > 120 ? -1 : framerate;
    }

    private void UpdateScreen() {
      var resolution = _resolutions[Resolution.Get()];
      var displayMode = DisplayMode.Get() switch {
        0 => FullScreenMode.ExclusiveFullScreen,
        1 => FullScreenMode.FullScreenWindow,
        _ => FullScreenMode.Windowed,
      };
      Screen.SetResolution(resolution.width, resolution.height, displayMode);
    }
  }
}
