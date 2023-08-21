using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

namespace Framework {
  public class GameManager : MonoBehaviour {
    [Inject] public MenuMode MenuMode;
    [Inject] public StoryMode StoryMode;
    [Inject] public AudioManager Audio;

    private GameMode _currentMode;
    private GameMode _switchingTo;

    private bool ISSwitching => !ReferenceEquals(_switchingTo, null);

    public void RequestMode(GameMode mode) {
      if (_switchingTo == mode || _currentMode == mode) {
        return;
      }

      StartCoroutine(SwitchMode(mode));
    }

    public void Quit() {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
      Application.Quit();
    }

    private void Awake() {
      Time.timeScale = 0;
      MenuMode.Setup(this);
      StoryMode.Setup(this);

#if UNITY_EDITOR
      switch (SceneManager.GetActiveScene().buildIndex) {
        case 0:
          RequestMode(MenuMode);
          break;
        case 1:
          _currentMode = MenuMode;
          _currentMode.OnEditorStart();
          break;
        default:
          _currentMode = StoryMode;
          _currentMode.OnEditorStart();
          break;
      }
#else
      RequestMode(MenuMode);
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update() {
      if (Keyboard.current.rKey.wasPressedThisFrame) {
        StartCoroutine(Reload());
      }

      if (Keyboard.current.fKey.wasPressedThisFrame) {
        Time.timeScale = Time.timeScale < 1 ? 1 : 0.2f;
      }
    }

    private IEnumerator Reload() {
      var scene = SceneManager.GetActiveScene().buildIndex;
      yield return SceneManager.UnloadSceneAsync(scene);
      yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

      SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(scene));
    }
#endif

    private IEnumerator SwitchMode(GameMode mode) {
      if (_switchingTo == mode || _currentMode == mode) {
        yield break;
      }

      if (ISSwitching) {
        yield return new WaitUntil(() => !ISSwitching);
      }

      _switchingTo = mode;
      if (!ReferenceEquals(_currentMode, null)) {
        yield return _currentMode.OnEnd();
      }

      _currentMode = mode;
      yield return _currentMode.OnStart();

      _switchingTo = null;
    }
  }
}
