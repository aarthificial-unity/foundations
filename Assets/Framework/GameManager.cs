using Saves;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Framework {
  public class GameManager : MonoBehaviour {
    [NonSerialized] public MenuState Menu;
    [NonSerialized] public StoryState Story;

    private GameState _currentState;
    private GameState _requestedState;

    public void DrivenAwake() {
      Menu = GetComponent<MenuState>();
      Story = GetComponent<StoryState>();

#if UNITY_EDITOR
      var currentIndex = SceneManager.GetActiveScene().buildIndex;
      if (currentIndex == 0) {
        SwitchState(Menu);
      } else {
        var task = App.Save.GetEditorSave();
        task.Wait(5000);
        if (!task.IsCompletedSuccessfully) {
          throw new ApplicationException();
        }
        Story.Enter(task.Result);
      }
#else
      SwitchState(Menu);
#endif
      StartCoroutine(Run());
    }

    public void SwitchState(GameState state) {
      _requestedState = state;
    }

    private IEnumerator Run() {
      while (true) {
        if (_currentState != null) {
          yield return _currentState.OnUpdate();
        }

        if (_currentState == _requestedState) {
          yield return null;
          continue;
        }

        if (_currentState != null) {
          yield return _currentState.OnExit();
        }
        _currentState = _requestedState;
        yield return _currentState.OnEnter();
      }
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
