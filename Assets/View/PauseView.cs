using FMODUnity;
using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace View {
  public class PauseView : MonoBehaviour {
    [SerializeField] [Inject] private StoryMode _storyMode;
    [SerializeField] [Inject] private MenuMode _menuMode;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private ButtonNavigation _resumeButton;
    [SerializeField] private ButtonNavigation _menuButton;
    [SerializeField] private ButtonNavigation _exitButton;
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private InputActionReference _resumeAction;
    [SerializeField] private StudioEventEmitter _pauseSound;

    private void OnEnable() {
      _resumeButton.Button.onClick.AddListener(_storyMode.Resume);
      _menuButton.Button.onClick.AddListener(_menuMode.RequestStart);
      _exitButton.Button.onClick.AddListener(_menuMode.Quit);
      _pauseAction.action.performed += HandlePauseAction;
      _resumeAction.action.performed += HandleResumeAction;
      _storyMode.Paused += HandlePaused;
      _storyMode.Resumed += HandleResumed;
      _resumeButton.QuickSelect();
      if (_storyMode.IsPaused) {
        HandlePaused();
      } else {
        HandleResumed();
      }
    }

    private void OnDisable() {
      _pauseAction.action.performed -= HandlePauseAction;
      _resumeAction.action.performed -= HandleResumeAction;
      _storyMode.Paused -= HandlePaused;
      _storyMode.Resumed -= HandleResumed;
    }

    private void HandlePaused() {
      _canvas.SetActive(true);
      _resumeButton.QuickSelect();
      _pauseSound.Play();
    }

    private void HandleResumed() {
      _canvas.SetActive(false);
    }

    private void HandlePauseAction(InputAction.CallbackContext _) {
      _storyMode.Pause();
    }

    private void HandleResumeAction(InputAction.CallbackContext _) {
      _pauseSound.Play();
      _storyMode.Resume();
    }
  }
}
