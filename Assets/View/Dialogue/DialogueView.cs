using Interactions;
using Player;
using System;
using UnityEngine;
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
    }

    public void DrivenUpdate(Vector3 lt, Vector3 rt) {
      var ratio = Screen.width / (float)Screen.height;
      if (ratio > 16 / 9f) {
        _canvas.scaleFactor = Screen.height / 1080f;
      } else {
        _canvas.scaleFactor = Screen.width / 1920f;
      }

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
      }

      PlayerFrame = _presentPlayers switch {
        PlayerType.LT => Rect.MinMaxRect(
          ScreenPosition.LT.x - capsuleSize.x,
          ScreenPosition.LT.y - capsuleSize.y,
          ScreenPosition.LT.x + capsuleSize.x,
          ScreenPosition.LT.y + capsuleSize.y
        ),
        PlayerType.RT => Rect.MinMaxRect(
          ScreenPosition.RT.x - capsuleSize.x,
          ScreenPosition.RT.y - capsuleSize.y,
          ScreenPosition.RT.x + capsuleSize.x,
          ScreenPosition.RT.y + capsuleSize.y
        ),
        _ => Rect.MinMaxRect(
          Mathf.Min(ScreenPosition.LT.x, ScreenPosition.RT.x) - capsuleSize.x,
          Mathf.Min(ScreenPosition.LT.y, ScreenPosition.RT.y) - capsuleSize.y,
          Mathf.Max(ScreenPosition.LT.x, ScreenPosition.RT.x) + capsuleSize.x,
          Mathf.Max(ScreenPosition.LT.y, ScreenPosition.RT.y) + capsuleSize.y
        ),
      };

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
