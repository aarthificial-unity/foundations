using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Items {
  public class ItemSlot : MonoBehaviour,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler {
    public event Action Dropped;

    [SerializeField] private Image _icon;
    private RectTransform _draggableTransform;
    private Vector2 _initialPosition;
    private Canvas _canvas;

    private void Awake() {
      _draggableTransform = _icon.GetComponent<RectTransform>();
      _canvas = GetComponentInParent<Canvas>();
      _initialPosition = _draggableTransform.anchoredPosition;
    }

    public void SetItem(Item item) {
      _icon.sprite = item == null ? null : item.Icon;
      _icon.enabled = item != null;
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData) {
      _draggableTransform.anchoredPosition +=
        eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
      if (Vector2.Distance(
          _draggableTransform.anchoredPosition,
          _initialPosition
        )
        > 60 / _canvas.scaleFactor) {
        Dropped?.Invoke();
      }

      _draggableTransform.anchoredPosition = _initialPosition;
    }
  }
}
