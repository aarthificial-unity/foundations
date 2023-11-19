using UnityEngine;

namespace Interactions {
  public class InteractionHighlight : MonoBehaviour {
    [SerializeField] private Material _material;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private Renderer[] _renderers;

    private Material[] _originalMaterials;

    private void Awake() {
      _originalMaterials = new Material[_renderers.Length];
      for (var i = 0; i < _renderers.Length; i++) {
        _originalMaterials[i] = _renderers[i].material;
      }
    }

    private void OnEnable() {
      _interactable.StateChanged += HandleStateChanged;
    }

    private void OnDisable() {
      _interactable.StateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged() {
      if (_interactable.IsHovered) {
        for (var i = 0; i < _renderers.Length; i++) {
          _renderers[i].material = _material;
        }
      } else {
        for (var i = 0; i < _renderers.Length; i++) {
          _renderers[i].material = _originalMaterials[i];
        }
      }
    }
  }
}
