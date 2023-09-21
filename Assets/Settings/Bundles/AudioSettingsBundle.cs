using UnityEngine;

namespace Settings.Bundles {
  public class AudioSettingsBundle : ScriptableObject {
    public SettingProperty MasterVolume;
    public SettingProperty MusicVolume;
    public SettingProperty SfxVolume;

    private void OnEnable() {
      MasterVolume = new SettingProperty("masterVolume", 100);
      MusicVolume = new SettingProperty("musicVolume", 100);
      SfxVolume = new SettingProperty("sfxVolume", 100);
    }
  }
}
