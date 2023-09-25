using UnityEngine;

namespace View {
  [DefaultExecutionOrder(-300)]
  [RequireComponent(typeof(Canvas))]
  public class AutoCanvasScale : MonoBehaviour {
    private Canvas _canvas;

    private void Awake() {
      _canvas = GetComponent<Canvas>();
    }

    private void Update() {
      var ratio = Screen.width / (float)Screen.height;
      if (ratio > 16 / 9f) {
        _canvas.scaleFactor = Screen.height / 1080f;
      } else {
        _canvas.scaleFactor = Screen.width / 1920f;
      }
    }
  }
}
