using Player;
using System;
using UnityEngine;
using Utils;
using Utils.Tweening;
using View.Overlay;

namespace Interactions {
  public class InteractionGizmo : MonoBehaviour {
    private static readonly int _stateID = Shader.PropertyToID("_State");
    private static readonly int
      _directionID = Shader.PropertyToID("_Direction");

    [SerializeField] [Range(0, 1)] private float _expansion;
    [SerializeField] [Range(0, 1)] private float _opacity;
    [SerializeField] [Range(0, 1)] private float _playerType;
    [SerializeField] [Range(0, 1)] private float _playerPresence;
    [Inject] [SerializeField] private PlayerChannel _players;
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] private Conversation _conversation;
    [SerializeField] private float _angle;

    private MeshRenderer _renderer;
    private MaterialPropertyBlock _block;
    private Dynamics _dynamics;

    private void Awake() {
      _renderer = GetComponent<MeshRenderer>();
      _block = new MaterialPropertyBlock();
    }

    private void OnEnable() {
      _conversation.StateChanged += Render;
      _players.Manager.DialogueState.Entered += Render;
      _players.Manager.DialogueState.Exited += Render;
      Render();
    }

    private void OnDisable() {
      _conversation.StateChanged -= Render;
      if (_players.Manager != null) {
        _players.Manager.DialogueState.Entered -= Render;
        _players.Manager.DialogueState.Exited -= Render;
      }
    }

    private void Render() {
      if (_players.Manager.DialogueState.IsActive) {
        _opacity = 0.32f;
        _expansion = 1;
        _playerPresence = 0;
        return;
      }

      var isExpanded = _conversation.IsInteracting && _conversation.HasDialogue;
      _expansion = isExpanded
        ? _conversation.IsHovered ? -0.2f : 0
        : _conversation.IsHovered
          ? 0.85f
          : 1;
      _opacity = _conversation.IsFocused
        ? 1
        : _conversation.IsHovered
          ? 0.54f
          : 0.32f;
      _playerType = _conversation.PlayerType switch {
        PlayerType.LT => 0,
        PlayerType.RT => 1,
        _ => 0.5f,
      };
      _playerPresence = _conversation.IsFocused ? 1 : 0;
    }

    private void Update() {
#if UNITY_EDITOR
      _block ??= new MaterialPropertyBlock();
#endif

      _dynamics.Set(
        new Vector4(_expansion, _opacity, _playerType, _playerPresence)
      );
      _block.SetVector(_stateID, _dynamics.Update(SpringConfig.Snappy));

      if (_players.IsReady) {
        var ltPosition =
          (Vector2)_overlay.CameraManager.MainCamera.WorldToScreenPoint(
            _players.LT.transform.position
          );
        var rtPosition =
          (Vector2)_overlay.CameraManager.MainCamera.WorldToScreenPoint(
            _players.RT.transform.position
          );

        var direction = (rtPosition - ltPosition).normalized;

        _block.SetVector(
          _directionID,
          new Vector4(direction.x, direction.y, 0, 0)
        );
      }
      _renderer.SetPropertyBlock(_block);
    }
  }
}
