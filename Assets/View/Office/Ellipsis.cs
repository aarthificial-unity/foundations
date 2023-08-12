using TMPro;
using UnityEngine;

namespace View.Office {
  public class Ellipsis : MonoBehaviour {
    [SerializeField] private float _interval = 0.5f;
    private TextMeshProUGUI _text;

    private void Awake() {
      _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update() {
      var time = Time.unscaledTime;
      var value = Mathf.FloorToInt(time / _interval) % 4;
      _text.maxVisibleCharacters = _text.textInfo.characterCount - 3 + value;
    }
  }
}
