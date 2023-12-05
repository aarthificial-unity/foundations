using Audio.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.Controls {
  public class PaperToggle : Toggle {
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private TextMeshProUGUI _tick;
    [SerializeField] private FMODEventInstance _onSound;
    [SerializeField] private FMODEventInstance _offSound;

    private PaperStyle _style;

    protected override void Awake() {
      base.Awake();
      _onSound.Setup();
      _offSound.Setup();
      _style = GetComponent<PaperStyle>();
      onValueChanged.AddListener(HandleValueChanged);
      HandleValueChanged(isOn);
    }

    protected override void OnDestroy() {
      _onSound.Release();
      _offSound.Release();
      base.OnDestroy();
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      _style?.DoStateTransition((PaperStyle.SelectionState)state);
    }

    private void HandleValueChanged(bool value) {
      _label.text = value ? "Enabled" : "Disabled";
      _tick.text = value ? "\ue2e6" : "\ue836";
      if (IsInteractable()) {
        if (value) {
          _onSound.Play();
        } else {
          _offSound.Play();
        }
      }
    }
  }
}
