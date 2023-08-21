using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View.Controls {
  public class PaperSlider : MonoBehaviour {
    public event Action<int> ValueChanged;

    [SerializeField] private float _min;
    [SerializeField] private float _max = 100;
    [SerializeField] private float _default = 50;
    [SerializeField] private float _step = 1;
    [SerializeField] private TextMeshProUGUI[] _labels;
    [SerializeField] private SerializedDictionary<float, string> _labelMap;
    private Slider _slider;
    private string _template;
    private bool _isHovered;

    public int Value {
      get =>
        Mathf.RoundToInt(_step > 0 ? _slider.value * _step : _slider.value);
      set => _slider.value = _step > 0 ? value / _step : value;
    }

    private void Awake() {
      _slider = GetComponent<Slider>();
      if (_step > 0) {
        _slider.wholeNumbers = true;
        _slider.minValue = Mathf.Floor(_min / _step);
        _slider.maxValue = Mathf.Ceil(_max / _step);
        _slider.value = Mathf.Round(_default / _step);
      } else {
        _slider.wholeNumbers = false;
        _slider.minValue = _min;
        _slider.maxValue = _max;
        _slider.value = _default;
      }
      _template = _labels[0].text;
      _slider.onValueChanged.AddListener(HandleValueChanged);
      HandleValueChanged(_slider.value);
    }

    private void HandleValueChanged(float _) {
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
    }

    public void SetInteractive(bool value) {
      _slider.interactable = value;
    }
  }
}
