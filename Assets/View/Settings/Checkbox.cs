using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.Settings {
  public class Checkbox : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private TextMeshProUGUI _tick;
    private Toggle _toggle;

    private void Awake() {
      _toggle = GetComponent<Toggle>();
      _toggle.onValueChanged.AddListener(HandleValueChanged);
      HandleValueChanged(_toggle.isOn);
    }

    private void HandleValueChanged(bool value) {
      _label.text = value ? "Enabled" : "Disabled";
      _tick.text = value ? "\ue2e6" : "\ue836";
    }
  }
}
