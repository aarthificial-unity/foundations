using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Framework {
  public class StoryState : GameState {
    [NonSerialized] public bool IsPaused;
    public event Action Paused;
    public event Action Resumed;
    public event Action Reloaded;

    private int _activeScene = 1;

    public void Enter() {
      Manager.SwitchState(this);
    }

    public override void OnEnter() {
      base.OnEnter();
      IsPaused = false;
      _activeScene = App.Save.Current.SceneIndex;
      if (SceneManager.GetActiveScene().buildIndex != _activeScene) {
        SceneManager.LoadScene(_activeScene);
      }
      Resume();
    }

    public void Reload() {
      Assert.IsTrue(IsActive);

      SceneManager.LoadScene(_activeScene);
      Reloaded?.Invoke();
    }

    public void SwapScene(string scenePath) {
      Assert.IsTrue(IsActive);

      SceneManager.LoadScene(scenePath);
      _activeScene = SceneManager.GetSceneByPath(scenePath).buildIndex;
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

      App.Audio.PlayAmbience();
      App.Input.SwitchToGameplay();
      IsPaused = false;
      Time.timeScale = 1;
      Resumed?.Invoke();
    }
  }
}
