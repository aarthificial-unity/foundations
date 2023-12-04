using Settings.Bundles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using View.Controls;

namespace View.Settings {
  [DefaultExecutionOrder(100)]
  public class GameplaySettingsView : MonoBehaviour {
    [Inject] [SerializeField] private GameplaySettingsBundle _bundle;

    [SerializeField] private Toggle _cameraShake;
    [SerializeField] private PaperSlider _cameraWeight;
    [SerializeField] private PaperToggle _autoDialogue;

    private void OnEnable() {
      _cameraShake.isOn = _bundle.CameraShake.GetBool();
      _cameraWeight.Value = _bundle.CameraWeight.Get();
      _autoDialogue.isOn = _bundle.AutoDialogue.GetBool();

      _cameraShake.onValueChanged.AddListener(HandleCameraShakeChanged);
      _cameraWeight.ValueChanged += HandleCameraWeightChanged;
      _autoDialogue.onValueChanged.AddListener(HandleAutoDialogueChanged);
    }

    private void OnDisable() {
      _cameraShake.onValueChanged.RemoveListener(HandleCameraShakeChanged);
      _cameraWeight.ValueChanged -= HandleCameraWeightChanged;
      _autoDialogue.onValueChanged.RemoveListener(HandleAutoDialogueChanged);
    }

    private void HandleCameraShakeChanged(bool value) {
      _bundle.CameraShake.Set(value);
    }

    private void HandleCameraWeightChanged(int value) {
      _bundle.CameraWeight.Set(value);
    }

    private void HandleAutoDialogueChanged(bool value) {
      _bundle.AutoDialogue.Set(value);
    }
  }
}
