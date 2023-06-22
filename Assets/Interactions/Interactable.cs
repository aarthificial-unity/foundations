using System;
using Player;
using UnityEngine;
using UnityEngine.Assertions;

namespace Interactions {
  public class Interactable : MonoBehaviour {
    [NonSerialized] public bool IsInteracting;
    [NonSerialized] public bool IsFocused;
    [NonSerialized] public bool IsHovered;

    [SerializeField]
    private PlayerType _playerType = PlayerType.LT | PlayerType.RT;

    private PlayerController _player;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _canInteract;

    private void Awake() {
      _propertyBlock = new MaterialPropertyBlock();
      _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start() {
      _canInteract = CanInteract();
      Render();
    }

    private void Update() {
      var canInteract = CanInteract();
      if (_canInteract != canInteract) {
        _canInteract = canInteract;
        Render();
      }
    }

    public virtual Vector3 GetPosition() {
      return transform.position;
    }

    public virtual float GetRadius() {
      return 0.3f;
    }

    public virtual bool CanInteract() {
      return true;
    }

    public virtual bool IsCompatibleWith(PlayerController player) {
      return (_playerType & player.Type) != 0;
    }

    public virtual void OnFocusEnter(PlayerController player) {
      Assert.IsNull(
        _player,
        "Trying to start interaction while already interacting"
      );
      IsFocused = true;
      _player = player;
      Render();
    }

    public virtual void OnFocusExit(PlayerController player) {
      Assert.AreEqual(
        player,
        _player,
        "Stop was called by a different player than Start"
      );
      IsFocused = false;
      _player = null;
      Render();
    }

    public void OnHoverEnter() {
      IsHovered = true;
      Render();
    }

    public void OnHoverExit() {
      IsHovered = false;
      Render();
    }

    public void OnInteractEnter() {
      IsInteracting = true;
      Render();
    }

    public void OnInteractExit() {
      IsInteracting = false;
      Render();
    }

    private void Render() {
      var playerCode = (int)_playerType;
      if (IsFocused) {
        playerCode = (int)_player.Type;
      }

      _propertyBlock.SetInt("_PlayerCode", playerCode);
      _propertyBlock.SetInt("_PlayerType", (int)_playerType);
      _propertyBlock.SetVector(
        "_State",
        new Vector4(
          IsHovered ? 1 : 0,
          IsFocused ? 1 : 0,
          IsInteracting ? 1 : 0,
          _canInteract ? 1 : 0
        )
      );
      _meshRenderer.SetPropertyBlock(_propertyBlock);
    }
  }
}
