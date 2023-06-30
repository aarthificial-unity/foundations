using System;
using Player;
using UnityEngine;

namespace Interactions {
  public class InteractionHandle : MonoBehaviour {
    [SerializeField] private Interactable _interactable;

    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;

    protected void Awake() {
      _propertyBlock = new MaterialPropertyBlock();
      _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update() {
      transform.rotation = Quaternion.Euler(45, 45, 0);
    }

    private void OnEnable() {
      _interactable.StateChanged += Render;
      Render();
    }

    private void OnDisable() {
      _interactable.StateChanged -= Render;
    }

    private void Render() {
      _propertyBlock.SetVector(
        "_State",
        new Vector4(
          _interactable.IsHovered ? 1 : 0,
          _interactable.IsFocused ? 1 : 0,
          _interactable.IsInteracting ? 1 : 0,
          (int)_interactable.PlayerType
        )
      );
      _meshRenderer.SetPropertyBlock(_propertyBlock);
    }
  }
}
