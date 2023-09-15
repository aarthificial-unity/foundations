using Player;
using TMPro;
using Typewriter;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Tweening;

namespace View.Dialogue {
  public class DialogueBubble : MonoBehaviour {
    public TextMeshProUGUI Text;
    [Inject] [SerializeField] private ButtonStyle _style;
    [SerializeField] private RectTransform _bubbleTransform;
    [SerializeField] private HorizontalLayoutGroup _rootLayout;
    [SerializeField] private BoxSDF _backgroundBox;
    [SerializeField] private BoxSDF _fillBox;
    [SerializeField] private Image _arrow;
    [SerializeField] private RectMask2D _arrowGroup;
    [SerializeField] private float spacing;

    private LayoutElement _layoutElement;
    private SpringTween _layoutTween;
    private PlayerController _player;
    private DialogueView _view;
    private Cached<bool> _isOnLeft;

    public void DrivenAwake(DialogueView view) {
      _view = view;
      _layoutElement = GetComponent<LayoutElement>();
    }

    public void DrivenUpdate() {
      if (_isOnLeft.HasChanged(
          _view.ScreenPosition.LT.x < _view.ScreenPosition.RT.x
            ? _player.IsLT
            : !_player.IsLT
        )) {
        _bubbleTransform.pivot = new Vector2(_isOnLeft ? 1 : 0, 1);
        _rootLayout.childAlignment = _isOnLeft
          ? TextAnchor.UpperRight
          : TextAnchor.UpperLeft;
        _arrowGroup.rectTransform.anchorMin = new Vector2(_isOnLeft ? 1 : 0, 1);
        _arrowGroup.rectTransform.anchorMax = new Vector2(_isOnLeft ? 1 : 0, 1);
        _arrowGroup.rectTransform.anchoredPosition = new Vector2(
          _isOnLeft ? -4 : 4,
          -37
        );
      }

      _layoutTween.Update(SpringConfig.Medium);
      var layout = _layoutTween.Value;
      var eyeOffset = _view.WorldToCanvas.y * (_player.IsLT ? 0.9f : 0.6f);
      var baseLayout = new Vector2(
        0,
        _view.ScreenPosition[_player.Type].y
        - _view.PlayerFrame.yMin
        + eyeOffset
      );
      var mainLayout = new Vector2(
        _view.PlayerFrame.height,
        _view.ScreenPosition[_player.Type].y
        - _view.PlayerFrame.yMax
        + eyeOffset
      );
      var storedLayout = new Vector2(
        _bubbleTransform.rect.height + spacing,
        -spacing / 2
      );
      var finalLayout = Vector2.Lerp(
        Vector2.Lerp(baseLayout, mainLayout, layout.x),
        storedLayout,
        layout.y
      );

      _layoutElement.minHeight = _layoutElement.preferredHeight = finalLayout.x;
      _bubbleTransform.anchoredPosition = new Vector2(
        _view.PlayerFrame.center.x
        + _view.PlayerFrame.width / (_isOnLeft ? -2 : 2),
        finalLayout.y
      );
      _arrowGroup.rectTransform.localScale = Vector3.Lerp(
        Vector3.one,
        Vector3.zero,
        layout.y
      );
      _arrowGroup.padding = new Vector4(
        _isOnLeft ? 25 * (1 - layout.y) : 0,
        0,
        _isOnLeft ? 0 : 25 * (1 - layout.y),
        0
      );
    }

    public void Store() {
      _layoutTween.Set(1, 1);
    }

    public void Setup(
      string text,
      DialogueEntry.BubbleStyle style,
      PlayerController player
    ) {
      _player = player;
      Text.text = text;
      Text.maxVisibleCharacters = 0;
      _layoutElement.minHeight = _layoutElement.preferredHeight = 0;

      var settings = _style[style];
      var backgroundColor = settings.BackgroundColors[_player.Type];
      var textColor = settings.TextColors[_player.Type];

      Text.color = textColor;
      _arrow.color = backgroundColor;
      _backgroundBox.Color = backgroundColor;
      _backgroundBox.TextureStrength = settings.TextureStrength;
      _fillBox.Color = backgroundColor;
      _fillBox.TextureStrength = settings.TextureStrength;
      _fillBox.Dash = settings.Stroke ? 4 : 0;

      _layoutTween.ForceSet(0, 0);
      _layoutTween.Set(1, 0);
    }

    public void SetCompletion(float percentage) {
      Text.maxVisibleCharacters =
        (int)(Text.textInfo.characterCount * percentage);
    }
  }
}
