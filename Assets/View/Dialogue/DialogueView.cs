using Interactions;
using Player;
using UnityEngine;
using Utils;
using View.Overlay;

namespace View.Dialogue {
  public class DialogueView : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] private RectTransform _frame;
    public DialogueTrack Track;
    public DialogueWheel Wheel;

    public PlayerLookup<Vector2> ScreenPosition;
    public Rect PlayerFrame;
    public Vector2 CanvasSize;
    public Vector2 WorldToCanvas;

    private CanvasGroup _canvasGroup;
    private InteractionContext _context;
    private PlayerType _presentPlayers;
    private Canvas _canvas;

    private void Awake() {
      _context = null;
      _canvasGroup = GetComponent<CanvasGroup>();
      _canvas = GetComponentInParent<Canvas>();
      _overlay.Dialogue = this;
      SetActive(false);
    }

    public void SetContext(InteractionContext context) {
      Debug.Log("Context set");
      _context = context;
    }

    public void DrivenUpdate(Vector3 lt, Vector3 rt) {
      var ratio = Screen.width / (float)Screen.height;
      if (ratio > 16 / 9f) {
        _canvas.scaleFactor = Screen.height / 1080f;
      } else {
        _canvas.scaleFactor = Screen.width / 1920f;
      }

      ScreenPosition.LT =
        _overlay.CameraManager.MainCamera.WorldToScreenPoint(lt)
        / _canvas.scaleFactor;
      ScreenPosition.RT =
        _overlay.CameraManager.MainCamera.WorldToScreenPoint(rt)
        / _canvas.scaleFactor;

      CanvasSize = _canvas.pixelRect.size / _canvas.scaleFactor;
      WorldToCanvas =
        _overlay.CameraManager.MainCamera.projectionMatrix.MultiplyVector(
          Vector3.one
        )
        * CanvasSize
        / 2;
      var capsuleSize = WorldToCanvas * new Vector2(0.5f, 1f);

      if (_context != null) {
        _presentPlayers = _context.Interactable.PlayerType;
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

    private void OnDestroy() {
      _overlay.Dialogue = null;
    }

    public void SetActive(bool value) {
      _canvasGroup.interactable = value;
      _canvasGroup.blocksRaycasts = value;
      _canvasGroup.alpha = value ? 1 : 0;
    }
  }
}
