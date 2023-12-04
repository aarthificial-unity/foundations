using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View.Dialogue {
  public class DialogueScrollViewport : MonoBehaviour,
    IDragHandler,
    IPointerDownHandler,
    IPointerUpHandler {
    public event Action Clicked;

    public bool IsPressed;

    public void OnDrag(PointerEventData eventData) {
      eventData.Use();
    }

    public void OnPointerDown(PointerEventData eventData) {
      IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
      Clicked?.Invoke();
      IsPressed = false;
    }
  }
}
