using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Tweening;

namespace View.Dialogue {
  public class DialogueBubble : MonoBehaviour {
    public TextMeshProUGUI Text;
    [SerializeField] private RectTransform _bubbleTransform;
    [SerializeField] private HorizontalLayoutGroup _rootLayout;
    [SerializeField] private Image _background;
    [SerializeField] private Image _arrow;
    [SerializeField] private PlayerLookup<Color> _backgroundColors;
    [SerializeField] private float spacing;
    private LayoutElement _layoutElement;
    private Dynamics _heightDynamics;
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
        _arrow.rectTransform.anchorMin = new Vector2(_isOnLeft ? 1 : 0, 1);
        _arrow.rectTransform.anchorMax = new Vector2(_isOnLeft ? 1 : 0, 1);
      }

      var layout = _heightDynamics.Update(SpringConfig.Medium);
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
        _view.PlayerFrame.width / (_isOnLeft ? -2 : 2),
        finalLayout.y
      );
      _arrow.rectTransform.anchoredPosition = new Vector2(
        Mathf.Lerp(0, _isOnLeft ? -40 : 40, layout.y),
        _arrow.rectTransform.anchoredPosition.y
      );
    }

    public void Store() {
      _heightDynamics.Set(1, 1);
    }

    public void Setup(string text, PlayerController player) {
      _player = player;
      _heightDynamics.ForceSet(0, 0);
      Text.text = text;
      Text.maxVisibleCharacters = 0;
      _layoutElement.minHeight = _layoutElement.preferredHeight = 0;

      var color = _backgroundColors[_player.Type];
      _background.color = color;
      _arrow.color = color;

      _heightDynamics.Set(1, 0);
    }

    public void SetCompletion(float percentage) {
      Text.maxVisibleCharacters =
        (int)(Text.textInfo.characterCount * percentage);
    }
  }
}
