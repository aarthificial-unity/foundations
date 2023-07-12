using Interactions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace View.Dialogue {
  [RequireComponent(typeof(RectTransform))]
  public class DialogueButton : MonoBehaviour {
    public event UnityAction Clicked {
      add => _button.onClick.AddListener(value);
      remove => _button.onClick.RemoveListener(value);
    }

    private Canvas _canvas;
    private RectTransform _rectTransform;
    private Button _button;
    private Conversation conversation;

    private void Awake() {
      _canvas = GetComponentInParent<Canvas>();
      _button = GetComponent<Button>();
      _rectTransform = GetComponent<RectTransform>();
      gameObject.SetActive(false);
    }

    public void SetInteraction(Conversation conversation) {
      this.conversation = conversation;
      gameObject.SetActive(this.conversation != null);
    }

    private void LateUpdate() {
      if (conversation != null) {
        _rectTransform.anchoredPosition =
          Camera.main.WorldToScreenPoint(conversation.transform.position)
          / _canvas.scaleFactor;
      }
    }
  }
}
