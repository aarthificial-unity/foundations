using Saves;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Framework {
  public class GameManager : MonoBehaviour {
    [NonSerialized] public MenuState Menu;
    [NonSerialized] public StoryState Story;

    private GameState _currentState;

    public void DrivenAwake() {
      Menu = GetComponent<MenuState>();
      Story = GetComponent<StoryState>();

#if UNITY_EDITOR
      var currentIndex = SceneManager.GetActiveScene().buildIndex;
      if (currentIndex == 0) {
        SwitchState(Menu);
      } else {
        App.Save.Current = new SaveController { SceneIndex = currentIndex };
        SwitchState(Story);
      }
#else
      SwitchState(Menu);
#endif
    }

    public void SwitchState(GameState state) {
      if (_currentState == state) {
        return;
      }

      _currentState?.OnExit();
      _currentState = state;
      _currentState?.OnEnter();
    }

    public void Quit() {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#endif
      Application.Quit();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update() {
      if (Keyboard.current.rKey.wasPressedThisFrame
        && Keyboard.current.ctrlKey.isPressed) {
        Story.Reload();
      }

      if (Keyboard.current.fKey.wasPressedThisFrame
        && Keyboard.current.ctrlKey.isPressed) {
        Time.timeScale = Time.timeScale < 1 ? 1 : 0.2f;
      }
    }
#endif
  }
}
