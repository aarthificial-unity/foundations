using Player;
using System;
using UnityEngine;
using Utils;
using Utils.Tweening;
using View.Overlay;

namespace Interactions {
  [DefaultExecutionOrder(400)]
  public class InteractionGizmo : MonoBehaviour {
    public static readonly Vector4 DialogueIcon = new(1, 0, 0, 0);
    public static readonly Vector4 PlayIcon = new(0, 1, 0, 0);
    public static readonly Vector4 SkipIcon = new(0, 0, 1, 0);
    public static readonly Vector4 CancelIcon = new(0, 0, 0, 1);

    private static readonly int _stateID = Shader.PropertyToID("_State");
    private static readonly int _iconID = Shader.PropertyToID("_Icon");
    private static readonly int
      _directionID = Shader.PropertyToID("_Direction");

    [NonSerialized] public Vector3 DefaultPosition;
    [NonSerialized] public Vector3 DesiredPosition;
    [NonSerialized] public bool IsExpanded;
    [NonSerialized] public bool IsHovered;
    [NonSerialized] public bool IsFocused;
    [NonSerialized] public bool IsDisabled;
    [NonSerialized] public PlayerType PlayerType;
    [NonSerialized] public Vector4 Icon;
    [NonSerialized] public Vector2 Direction;

    [Inject] [SerializeField] private OverlayChannel _overlay;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _block;
    private SpringTween _stateTween;
    private SpringTween _playerTween;
    private SpringTween _iconTween;
    private SpringTween _positionTween;

    private void Awake() {
      _renderer = GetComponent<MeshRenderer>();
      _block = new MaterialPropertyBlock();
      DefaultPosition = transform.position;
      DesiredPosition = DefaultPosition;
    }

    public void MoveTo(Vector3 desiredPosition) {
      DesiredPosition = desiredPosition;
      var currentPosition = transform.position;
      _positionTween.ForceSet(currentPosition - desiredPosition);
      _positionTween.Set(0, 0, 0);
    }

    private void LateUpdate() {
#if UNITY_EDITOR
      _block ??= new MaterialPropertyBlock();
#endif

      var scale = _overlay.CameraManager.MainCamera.orthographicSize / 20f * 3;
      var ratio = Screen.width / (float)Screen.height;
      if (ratio < 16 / 9f) {
        scale *= ratio / 16f * 9f;
      }
      transform.localScale = Vector3.one * scale;

      var expansion = IsExpanded
        ? IsHovered ? -0.2f : 0
        : IsHovered
          ? 0.85f
          : 1;
      var opacity = IsFocused
        ? 1
        : IsHovered
          ? 0.54f
          : 0.32f;
      var playerPresence = IsFocused ? 1 : 0;
      var playerType = PlayerType switch {
        PlayerType.LT => 0,
        PlayerType.RT => 1,
        _ => 0.5f,
      };

      if (IsDisabled) {
        expansion = 1.3f;
        opacity = 0;
      }

      if (_stateTween.Value.z < 0.1f) {
        _playerTween.ForceSet(playerType);
      } else {
        _playerTween.Set(playerType);
      }

      _iconTween.Set(Icon);
      _stateTween.Set(expansion, opacity, playerPresence);

      _playerTween.Update(SpringConfig.Slow);
      _iconTween.Update(SpringConfig.Snappy);
      _stateTween.Update(SpringConfig.Snappy);
      var stateValue = _stateTween.Value;
      _block.SetVector(
        _stateID,
        new Vector4(stateValue.x, stateValue.y, _playerTween.X, stateValue.z)
      );
      _block.SetVector(
        _directionID,
        new Vector4(Direction.x, Direction.y, 0, 0)
      );
      _block.SetVector(_iconID, _iconTween.Value);

      _renderer.SetPropertyBlock(_block);

      _positionTween.Update(SpringConfig.Medium);
      transform.position = DesiredPosition + (Vector3)_positionTween.Value;
    }
  }
}
