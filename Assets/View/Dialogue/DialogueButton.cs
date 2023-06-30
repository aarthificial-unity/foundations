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
    private Interactable _interactable;

    private void Awake() {
      _canvas = GetComponentInParent<Canvas>();
      _button = GetComponent<Button>();
      _rectTransform = GetComponent<RectTransform>();
      gameObject.SetActive(false);
    }

    public void SetInteraction(Interactable interactable) {
      _interactable = interactable;
      gameObject.SetActive(_interactable != null);
    }

    private void LateUpdate() {
      if (_interactable != null) {
        _rectTransform.anchoredPosition =
          Camera.main.WorldToScreenPoint(_interactable.transform.position)
          / _canvas.scaleFactor;
      }
    }
  }
}
