using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Stores;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Blackboards;
using Interactions;
using Saves;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Utils;

namespace Framework {
  public class StoryState : GameState, ITypewriterContext {
    [NonSerialized] public bool IsPaused;
    public event Action<bool> LoadingChanged;
    public event Action Paused;
    public event Action Resumed;
    public event Action Reloaded;

    private bool _isLoading;
    public bool IsLoading {
      get => _isLoading;
      private set {
        if (_isLoading == value) {
          return;
        }
        _isLoading = value;
        LoadingChanged?.Invoke(_isLoading);
      }
    }

    private SaveController _saveController;
    private string _swapScene;
    private bool _saveScene;
    private bool _reloadScene;

    public void Enter(SaveController saveController) {
      _saveController = saveController;
      Manager.SwitchState(this);
    }

    public override IEnumerator OnEnter() {
      Assert.IsNotNull(_saveController);

      yield return base.OnEnter();
      IsPaused = false;
      IsLoading = true;
      yield return _saveController.Create().AsIEnumerator();
      yield return _saveController.Load(SaveMode.Full).AsIEnumerator();
      if (SceneManager.GetActiveScene().name
        != _saveController.GlobalData.SceneName) {
        yield return SceneManager.LoadSceneAsync(
          _saveController.GlobalData.SceneName
        );
      }
      yield return _saveController.Load().AsIEnumerator();
      IsLoading = false;
      Resume();
    }

    public override IEnumerator OnExit() {
      while (_saveController.IsLoading) {
        yield return null;
      }

      yield return base.OnExit();
      SaveStoreRegistry.Clear();
      _saveController = null;
      _saveScene = false;
      _reloadScene = false;
      _swapScene = null;
    }

    public override IEnumerator OnUpdate() {
      if (_saveScene) {
        IsLoading = true;
        yield return _saveController.Save(SaveMode.Full).AsIEnumerator();
        IsLoading = false;
        _saveScene = false;
      }

      if (_swapScene != null) {
        IsLoading = true;
        yield return SwapSceneCoroutine(_swapScene);
        IsLoading = false;
        _swapScene = null;
      } else if (_reloadScene) {
        yield return SceneManager.LoadSceneAsync(
          _saveController.GlobalData.SceneName
        );
        yield return _saveController.Load().AsIEnumerator();
        _reloadScene = false;
        Reloaded?.Invoke();
      }
    }

    public void Reload() {
      _reloadScene = true;
    }

    public void SwapScene(string scenePath) {
      Assert.IsTrue(IsActive);
      _swapScene = scenePath;
    }

    private IEnumerator SwapSceneCoroutine(string scenePath) {
      _saveController.GlobalData.SceneName = scenePath;
      yield return _saveController.Save(SaveMode.Full).AsIEnumerator();
      yield return SceneManager.LoadSceneAsync(scenePath);
      yield return _saveController.Load().AsIEnumerator();
    }

    public void Pause() {
      Assert.IsTrue(IsActive);

      IsPaused = true;
      Time.timeScale = 0;
      App.Input.SwitchToUI();
      Paused?.Invoke();
    }

    public void Resume() {
      Assert.IsTrue(IsActive);

      App.Input.SwitchToGameplay();
      IsPaused = false;
      Time.timeScale = 1;
      Resumed?.Invoke();
    }

    public void AutoSave() {
      if (_saveController.AutoSave) {
        _saveScene = true;
      }
    }

    public void Save() {
      _saveScene = true;
    }

    public bool TryGetBlackboard(int scope, out IBlackboard blackboard) {
      if (scope == InteractionContext.GlobalScope) {
        blackboard = _saveController.GlobalData.Blackboard;
        return true;
      }

      blackboard = default;
      return false;
    }
  }
}
