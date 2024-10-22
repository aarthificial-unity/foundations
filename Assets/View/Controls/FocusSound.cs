﻿using Audio.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.Controls {
  public class FocusSound : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler,
    IDeselectHandler,
    ISelectHandler {
    public FMODEventInstance Focus => _focusSound;
    public FMODEventInstance Blur => _blurSound;

    [SerializeField] private FMODEventInstance _focusSound;
    [SerializeField] private FMODEventInstance _blurSound;
    private Selectable _selectable;
    private bool _ignoreEvents;

    private void Awake() {
      _focusSound.Setup();
      _blurSound.Setup();
      _selectable = GetComponent<Selectable>();
    }

    private void OnDestroy() {
      _focusSound.Release();
      _blurSound.Release();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      OnPointerMove(eventData);
    }

    public void OnPointerMove(PointerEventData eventData) {
      if (EventSystem.current != null
        && EventSystem.current.currentSelectedGameObject != gameObject
        && _selectable.IsInteractable()) {
        _selectable.Select();
      }
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (EventSystem.current != null
        && !EventSystem.current.alreadySelecting
        && EventSystem.current.currentSelectedGameObject == gameObject) {
        EventSystem.current.SetSelectedGameObject(null);
      }
    }

    public void OnSelect(BaseEventData eventData) {
      if (!_ignoreEvents) {
        _focusSound.Play();
      }
    }

    public void QuietSelect() {
      if (_selectable.IsInteractable()) {
        _ignoreEvents = true;
        _selectable.Select();
        _ignoreEvents = false;
      }
    }

    public void OnDeselect(BaseEventData eventData) {
      if (!_ignoreEvents && _selectable.IsInteractable()) {
        _blurSound.Play();
      }
    }
  }
}
