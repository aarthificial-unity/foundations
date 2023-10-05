using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Player;
using UnityEngine;
using Utils.Tweening;

namespace Interactions {
  public class TutorialInteraction : MonoBehaviour {
    [EntryFilter(Variant = EntryVariant.Event)]
    [SerializeField]
    private EntryReference _event;
    [SerializeField] private PlayerLookup<InteractionGizmo> _gizmos;
    [SerializeField] private float _hintDelay;

    private Conversation _conversation;
    private bool _isReady;
    private float _readyTime;
    private PlayerLookup<bool> _hintShown;

    private void Awake() {
      _conversation = GetComponent<Conversation>();
      SetupGizmo(PlayerType.LT);
      SetupGizmo(PlayerType.RT);
    }

    private void SetupGizmo(PlayerType playerType) {
      var gizmo = _gizmos[playerType];
      gizmo.Mouse = new Vector4(
        1,
        playerType == PlayerType.LT ? 0 : 1,
        0.1f,
        0
      );
      gizmo.IsDisabled = true;
      gizmo.IsExpanded = true;
      gizmo.IsFocused = true;
      gizmo.Direction = new Vector2(1, 0);
      gizmo.PlayerType = playerType;
      gizmo.PositionSpring = SpringConfig.Bouncy;
    }

    private void OnEnable() {
      _conversation.StateChanged += HandleStateChanged;
    }

    private void OnDisable() {
      _conversation.StateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged() {
      if (!_conversation.IsDialogue && !_isReady) {
        _isReady = true;
        _conversation.SetEvent(0);
        _readyTime = Time.time;
      }

      if (!_isReady) {
        return;
      }

      if (!_conversation.PlayerType.HasFlag(PlayerType.LT)) {
        _hintShown.LT = true;
        _readyTime = Time.time;
        _gizmos.LT.IsDisabled = true;
      }

      if (!_conversation.PlayerType.HasFlag(PlayerType.RT)) {
        _hintShown.RT = true;
        _readyTime = Time.time;
        _gizmos.RT.IsDisabled = true;
      }
    }

    private void Update() {
      if (!_isReady || Time.time - _readyTime < _hintDelay) {
        return;
      }

      if (!_hintShown.LT) {
        _readyTime = Time.time;
        _gizmos.LT.IsDisabled = false;
        _gizmos.LT.Shake();
      } else if (!_hintShown.RT) {
        _readyTime = Time.time;
        _gizmos.RT.IsDisabled = false;
        _gizmos.RT.Shake();
      } else {
        enabled = false;
        _conversation.SetEvent(_event);
      }
    }
  }
}
