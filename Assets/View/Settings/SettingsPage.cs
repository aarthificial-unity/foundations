using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private Dynamics _positionDynamics;
    private Dynamics _angleDynamics;
    private Dynamics _colorDynamics;

    public void DrivenAwake(float spacing, int order, int count) {
      _spacing = spacing;
      _count = count;
      _canvas = GetComponent<Canvas>();
      _group = GetComponent<CanvasGroup>();
      _initialRotation = transform.localRotation.eulerAngles;
      _canvas.sortingOrder = order;
      _colorDynamics.ForceSet(order / (float)count);
      _angleDynamics.ForceSet(_angle);
      _positionDynamics.ForceSet(
        new Vector3(_position.x, order * spacing, _position.y)
      );
    }

    private void Update() {
      transform.localPosition =
        _positionDynamics.UnscaledUpdate(SpringConfig.Snappy);
      transform.localRotation = Quaternion.Euler(
        _initialRotation.x,
        _initialRotation.y,
        180 - _angleDynamics.UnscaledUpdate(SpringConfig.Snappy).x
      );
      _paper.color = Color.Lerp(
        _palette.Paper,
        _palette.PaperSelected,
        _colorDynamics.UnscaledUpdate(SpringConfig.Snappy).x
      );
    }

    public void SetOrder(int order) {
      _positionDynamics.Set(
        new Vector3(_position.x, order * _spacing, _position.y)
      );
      _canvas.sortingOrder = order;
      if (order == _count) {
        _colorDynamics.ForceSet(1);
      } else {
        _colorDynamics.Set(order / (float)_count);
      }
    }

    public void SetInteractive(bool value) {
      _group.interactable = value;
      _group.blocksRaycasts = value;
    }

    public void Nudge() {
      _positionDynamics.AddImpulse(new Vector3(-200, -100, 0));
      _angleDynamics.AddImpulse(200);
    }
  }
}
