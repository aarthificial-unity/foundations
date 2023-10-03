using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Tweening;
using View.Controls;

namespace View.Office.States {
  public class CreditsState : MenuState {
    [SerializeField] private CanvasGroup _label;
    [SerializeField] private Clickable _clickable;
    [SerializeField] private PaperButton _exitButton;
    [SerializeField] private Collider _collider;
    [SerializeField] private Renderer _stringRenderer;
    [SerializeField] private float _angleOffset;
    [SerializeField] private Transform[] _papers;
    [SerializeField] private SpringConfig _springConfig = new(70, 6);

    private CanvasGroup _exitButtonGroup;
    private SpringTween _paperTween;
    private float[] _angles;
    private Cached<bool> _isFocused;

    protected override void Awake() {
      _angles = new float[_papers.Length];
      for (var i = 0; i < _papers.Length; i++) {
        _angles[i] = _papers[i].localEulerAngles.z;
      }

      _clickable.Clicked += Enter;
      _clickable.StateChanged += HandleStateChanged;
      _exitButton.Clicked += Exit;
      _exitButtonGroup = _exitButton.GetComponent<CanvasGroup>();
      base.Awake();
    }

    public override void OnEnter() {
      base.OnEnter();
      App.Actions.UICancel.action.performed += HandleCancel;
      _clickable.interactable = false;
      _exitButtonGroup.interactable = true;
      HandleStateChanged();
    }

    public override void OnExit() {
      base.OnExit();
      App.Actions.UICancel.action.performed -= HandleCancel;
      _clickable.interactable = true;
      _exitButtonGroup.interactable = false;
      HandleStateChanged();
    }

    private void FixedUpdate() {
      if (_paperTween.Update(_springConfig)) {
        for (var i = 0; i < _papers.Length; i++) {
          var paper = _papers[i];
          paper.eulerAngles = new Vector3(
            0,
            0,
            _angles[i]
            + _paperTween.X * paper.transform.localPosition.x * _angleOffset
          );
        }
      }
    }

    protected override void OnProgress(float value) {
      _collider.enabled = value < 1f;
      _label.alpha = value.ClampRemap(0, 0.75f, 1, 0);
      _exitButtonGroup.alpha = value.ClampRemap(0.25f, 1, 0, 1);

      var color = _stringRenderer.sharedMaterial.color;
      color.a = value.Map(1f, 0.32f);
      _stringRenderer.material.color = color;
    }

    private void HandleCancel(InputAction.CallbackContext obj) {
      if (IsActive) {
        Manager.MainMenuState.Enter();
      }
    }

    private void HandleStateChanged() {
      if (_isFocused.HasChanged(_clickable.IsFocused) && _isFocused) {
        _paperTween.AddImpulse(_angleOffset);
      }
    }

    public void Enter() {
      Manager.SwitchState(this);
    }

    private void Exit() {
      Manager.MainMenuState.Enter();
    }
  }
}
