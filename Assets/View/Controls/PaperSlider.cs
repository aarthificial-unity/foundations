using Audio.Events;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.Controls {
  public class PaperSlider : Slider {
    public event Action<int> ValueChanged;

    [SerializeField] private float _min;
    [SerializeField] private float _max = 100;
    [SerializeField] private float _default = 50;
    [SerializeField] private float _step = 1;
    [SerializeField] private TextMeshProUGUI[] _labels;
    [SerializeField] private SerializedDictionary<float, string> _labelMap;
    [SerializeField] private FMODEventInstance _changeSound;
    private string _template;
    private PaperStyle _style;
    private float _lastSetTime;

    public int Value {
      get => Mathf.RoundToInt(_step > 0 ? value * _step : value);
      set => this.value = _step > 0 ? value / _step : value;
    }

    protected override void Awake() {
      base.Awake();
      _style = GetComponent<PaperStyle>();
      _changeSound.Setup();

      if (_step > 0) {
        wholeNumbers = true;
        value = Mathf.Round(_default / _step);
        minValue = Mathf.Floor(_min / _step);
        maxValue = Mathf.Ceil(_max / _step);
      } else {
        wholeNumbers = false;
        minValue = _min;
        maxValue = _max;
        value = _default;
      }
      _template = _labels[0].text;
      onValueChanged.AddListener(HandleValueChanged);
      HandleValueChanged(value);
    }

    protected override void OnDestroy() {
      _changeSound.Release();
      base.OnDestroy();
    }

    protected override void DoStateTransition(
      SelectionState state,
      bool instant
    ) {
      _style?.DoStateTransition((PaperStyle.SelectionState)state);
    }

    private void HandleValueChanged(float _) {
      if (!Application.isPlaying) {
        return;
      }

      var value = Value;

      if (_labelMap.TryGetValue(value, out var label)) {
        for (var i = 0; i < _labels.Length; i++) {
          _labels[i].SetText(label);
        }
      } else {
        for (var i = 0; i < _labels.Length; i++) {
          _labels[i].SetText(_template, value);
        }
      }

      ValueChanged?.Invoke(value);

      if (IsInteractable() && Time.unscaledTime - _lastSetTime > 0.05f) {
        _changeSound.Play();
        _lastSetTime = Time.unscaledTime;
      }
    }

    public void SetInteractive(bool value) {
      interactable = value;
    }
  }
}
