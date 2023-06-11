using Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;

namespace View {
  public class PauseView : MonoBehaviour {
    [SerializeField] [Inject] private StoryMode _storyMode;
    [SerializeField] [Inject] private MenuMode _menuMode;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private InputActionReference _resumeAction;

    private void OnEnable() {
      _resumeButton.onClick.AddListener(_storyMode.Resume);
      _menuButton.onClick.AddListener(_menuMode.RequestStart);
      _exitButton.onClick.AddListener(_menuMode.Quit);
      _pauseAction.action.performed += HandlePauseAction;
      _resumeAction.action.performed += HandleResumeAction;
      _storyMode.Paused += HandlePaused;
      _storyMode.Resumed += HandleResumed;
      _resumeButton.Select();
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
      _resumeButton.Select();
    }

    private void HandleResumed() {
      _canvas.SetActive(false);
    }

    private void HandlePauseAction(InputAction.CallbackContext _) {
      _storyMode.Pause();
    }

    private void HandleResumeAction(InputAction.CallbackContext _) {
      Debug.Log("Resume");
      _storyMode.Resume();
    }
  }
}
