using System;
using TMPro;
using UnityEngine;

namespace View.Dialogue {
  public class DialogueBubble : MonoBehaviour {
    public TextMeshProUGUI Text;
    [SerializeField] private RectTransform _bubbleTransform;
    private RectTransform _rectTransform;
    private Canvas _canvas;

    private void Awake() {
      _rectTransform = GetComponent<RectTransform>();
      _canvas = GetComponentInParent<Canvas>();
    }

    public void UpdatePosition(Vector3 position, bool isLeft) {
      _rectTransform.position = position * _canvas.scaleFactor;
      _bubbleTransform.pivot = new Vector2(isLeft ? 1 : 0, 0.5f);
      _bubbleTransform.anchoredPosition = new Vector2(isLeft ? -80 : 80, 0);
    }

    public void SetText(string text) {
      Text.text = text;
      Text.maxVisibleCharacters = 0;
    }

    public void SetCompletion(float percentage) {
      Text.maxVisibleCharacters =
        (int)(Text.textInfo.characterCount * percentage);
    }
  }
}
