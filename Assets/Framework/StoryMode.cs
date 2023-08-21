using System;
using System.Collections;
using Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Framework {
  public enum GameModeState {
    Loading,
    Starting,
    Started,
    Ending,
    Ended,
  }

  public class StoryMode : GameMode {
    private const int _uiSceneIndex = 2;

    [NonSerialized] public bool IsPaused;
    public event Action Paused;
    public event Action Resumed;
    public event Action Reloaded;

    [SerializeField] [Inject] private InputChannel _input;
    private GameModeState _state = GameModeState.Ended;
    private int _activeScene = 3;

    public override void Setup(GameManager manager) {
      base.Setup(manager);
      _state = GameModeState.Ended;
    }

    public void RequestStart() {
      Manager.RequestMode(this);
    }

    public override IEnumerator OnStart() {
      if (_state != GameModeState.Ended) {
        yield break;
      }

      _state = GameModeState.Starting;
      IsPaused = false;
      Time.timeScale = 0;

      yield return SceneManager.LoadSceneAsync(
        _uiSceneIndex,
        LoadSceneMode.Additive
      );
      yield return SceneManager.LoadSceneAsync(
        _activeScene,
        LoadSceneMode.Additive
      );

      SceneManager.SetActiveScene(
        SceneManager.GetSceneByBuildIndex(_activeScene)
      );
      _state = GameModeState.Started;

      Resume();
    }

    public override IEnumerator OnEnd() {
      if (!IsStarted()) {
        yield break;
      }

      _state = GameModeState.Ending;
      Time.timeScale = 0;

      yield return SceneManager.UnloadSceneAsync(_activeScene);
      yield return SceneManager.UnloadSceneAsync(_uiSceneIndex);

      _state = GameModeState.Ended;
    }

    private bool IsStarted() {
      return _state == GameModeState.Started;
    }

    public void Reload() {
      if (IsStarted()) {
        Manager.StartCoroutine(ReloadCoroutine());
      }
    }

    private IEnumerator ReloadCoroutine() {
      _state = GameModeState.Loading;

      Time.timeScale = 0;
      yield return SceneManager.UnloadSceneAsync(_activeScene);
      yield return SceneManager.LoadSceneAsync(
        _activeScene,
        LoadSceneMode.Additive
      );

      SceneManager.SetActiveScene(
        SceneManager.GetSceneByBuildIndex(_activeScene)
      );
      Time.timeScale = 1;
      _state = GameModeState.Started;

      Reloaded?.Invoke();
    }

    public void Pause() {
      if (!IsStarted()) {
        return;
      }

      IsPaused = true;
      Time.timeScale = 0;
      _input.SwitchToUI();

      Paused?.Invoke();
    }

    public void Resume() {
      if (!IsStarted()) {
        return;
      }

      Manager.Audio.PlayAmbience();
      IsPaused = false;
      Time.timeScale = 1;
      _input.SwitchToGameplay();

      Resumed?.Invoke();
    }

#if UNITY_EDITOR
    public override void OnEditorStart() {
      var isPaused = SceneManager.GetSceneByBuildIndex(_uiSceneIndex).isLoaded;
      SceneManager.LoadScene(
        isPaused ? _activeScene : _uiSceneIndex,
        LoadSceneMode.Additive
      );

      if (!isPaused) {
        _activeScene = SceneManager.GetActiveScene().buildIndex;
      }

      _state = GameModeState.Started;

      if (isPaused) {
        Pause();
      } else {
        Resume();
      }
    }
#endif
  }
}
