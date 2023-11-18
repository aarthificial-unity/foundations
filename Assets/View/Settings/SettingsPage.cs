using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;
using Utils.Tweening;
using View.Office;

namespace View.Settings {
  public class SettingsPage : MonoBehaviour {
    [Inject] [SerializeField] private OfficePalette _palette;
    [FormerlySerializedAs("Position")]
    [SerializeField]
    private Vector2 _position;
    [FormerlySerializedAs("Angle")]
    [SerializeField]
    private float _angle;
    [SerializeField] private Image _paper;
    private Canvas _canvas;
    private CanvasGroup _group;
    private int _count;
    private float _spacing;

    private Vector3 _initialRotation;
    private SpringTween _positionTween;
    private SpringTween _angleTween;
    private SpringTween _colorTween;

    public void DrivenAwake(
      Camera worldCamera,
      float spacing,
      int order,
      int count
    ) {
      _spacing = -spacing;
      _count = count;
      _canvas = GetComponent<Canvas>();
      _group = GetComponent<CanvasGroup>();
      _canvas.worldCamera = worldCamera;
      _initialRotation = transform.localRotation.eulerAngles;
      _canvas.sortingOrder = order;
      _colorTween.ForceSet(order / (float)count);
      _angleTween.ForceSet(_angle);
      _positionTween.ForceSet(
        new Vector3(_position.x, _position.y, order * _spacing)
      );
    }

    private void Update() {
      if (_positionTween.UnscaledUpdate(SpringConfig.Snappy)) {
        transform.localPosition = _positionTween.Value;
      }

      if (_angleTween.UnscaledUpdate(SpringConfig.Snappy)) {
        transform.localRotation = Quaternion.Euler(
          _initialRotation.x,
          _initialRotation.y,
          180 - _angleTween.X
        );
      }

      if (_colorTween.UnscaledUpdate(SpringConfig.Snappy)) {
        _paper.color = Color.Lerp(
          _palette.Paper,
          _palette.PaperSelected,
          _colorTween.X
        );
      }
    }

    public void SetOrder(int order) {
      _positionTween.Set(
        new Vector3(_position.x, _position.y, order * _spacing)
      );
      _canvas.sortingOrder = order;
      if (order == _count) {
        _colorTween.ForceSet(1);
      } else {
        _colorTween.Set(order / (float)_count);
      }
    }

    public void SetInteractive(bool value) {
      _group.interactable = value;
      _group.blocksRaycasts = value;
    }

    public void Nudge() {
      _positionTween.AddImpulse(new Vector3(-200, -100, 0));
      _angleTween.AddImpulse(200);
    }
  }
}
