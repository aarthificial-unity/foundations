using Interactions;
using Player;
using System;
using UnityEngine;
using Utils.Tweening;
using View.Overlay;

namespace View.Dialogue {
  public class DialogueView : MonoBehaviour {
    [SerializeField] private RectTransform _frame;
    public DialogueTrack Track;
    public DialogueWheel Wheel;
    public InteractionContext Context;

    [NonSerialized] public PlayerLookup<Vector2> ScreenPosition;
    [NonSerialized] public Rect PlayerFrame;
    [NonSerialized] public Vector2 CanvasSize;
    [NonSerialized] public Vector2 WorldToCanvas;

    private CanvasGroup _canvasGroup;
    private PlayerType _presentPlayers;
    private Canvas _canvas;
    private Camera _mainCamera;
    private PlayerType _lastIndividualPlayer;
    private SpringTween _frameTween;

    private void Awake() {
      Context = null;
      _mainCamera = OverlayManager.Camera;
      _canvasGroup = GetComponent<CanvasGroup>();
      _canvas = GetComponentInParent<Canvas>();
      SetActive(false);
    }

    public void SetContext(InteractionContext context) {
      Context = context;
      Wheel.Button.SetGizmo(Context?.Interactable.Gizmo);
      if (Context != null) {
        _frameTween.ForceSet(
          Context.Interactable.PlayerType == PlayerType.Both ? 1 : 0
        );
      }
    }

    public void DrivenUpdate(Vector3 lt, Vector3 rt) {
      ScreenPosition.LT = _mainCamera.WorldToScreenPoint(lt)
        / _canvas.scaleFactor;
      ScreenPosition.RT = _mainCamera.WorldToScreenPoint(rt)
        / _canvas.scaleFactor;

      CanvasSize = _canvas.pixelRect.size / _canvas.scaleFactor;
      WorldToCanvas = _mainCamera.projectionMatrix.MultiplyVector(Vector3.one)
        * CanvasSize
        / 2;
      var capsuleSize = WorldToCanvas * new Vector2(0.5f, 1f);

      if (Context != null) {
        _presentPlayers = Context.Interactable.PlayerType;
        _lastIndividualPlayer = _presentPlayers switch {
          PlayerType.LT => PlayerType.LT,
          PlayerType.RT => PlayerType.RT,
          _ => _lastIndividualPlayer,
        };
        _frameTween.Set(_presentPlayers == PlayerType.Both ? 1 : 0);
      }

      var individualScreenPosition = ScreenPosition[_lastIndividualPlayer];
      var individualFrame = Rect.MinMaxRect(
        individualScreenPosition.x - capsuleSize.x,
        individualScreenPosition.y - capsuleSize.y,
        individualScreenPosition.x + capsuleSize.x,
        individualScreenPosition.y + capsuleSize.y
      );
      var bothFrame = Rect.MinMaxRect(
        Mathf.Min(ScreenPosition.LT.x, ScreenPosition.RT.x) - capsuleSize.x,
        Mathf.Min(ScreenPosition.LT.y, ScreenPosition.RT.y) - capsuleSize.y,
        Mathf.Max(ScreenPosition.LT.x, ScreenPosition.RT.x) + capsuleSize.x,
        Mathf.Max(ScreenPosition.LT.y, ScreenPosition.RT.y) + capsuleSize.y
      );

      _frameTween.Update(SpringConfig.Snappy);
      PlayerFrame = new Rect(
        Vector2.Lerp(
          individualFrame.position,
          bothFrame.position,
          _frameTween.X
        ),
        Vector2.Lerp(individualFrame.size, bothFrame.size, _frameTween.X)
      );

      _frame.offsetMin = PlayerFrame.min;
      _frame.offsetMax = PlayerFrame.max - CanvasSize;

      Wheel.DrivenUpdate();
      Track.DrivenUpdate();
    }

    public void SetActive(bool value) {
      _canvasGroup.interactable = value;
      _canvasGroup.blocksRaycasts = value;
      _canvasGroup.alpha = value ? 1 : 0;
    }
  }
}
