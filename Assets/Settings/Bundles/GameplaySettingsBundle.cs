using UnityEngine;

namespace Settings.Bundles {
  public class GameplaySettingsBundle : ScriptableObject {
    public SettingProperty CameraShake;
    public SettingProperty CameraWeight;
    public SettingProperty AutoDialogue;

    private void OnEnable() {
      CameraShake = new SettingProperty("cameraShake", 1);
      CameraWeight = new SettingProperty("cameraWeight", 60);
      CameraWeight = new SettingProperty("autoDialogue", 1);
    }
  }
}
