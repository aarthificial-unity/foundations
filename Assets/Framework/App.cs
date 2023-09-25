using Audio;
using FMODUnity;
using Input;
using Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework {
  public static class App {
    public static GameManager Game;
    public static InputManager Input;
    public static AudioManager Audio;
    public static SaveManager Save;
    public static InputActions Actions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BetterBootstrap() {
#if UNITY_EDITOR
      if (ShouldSkip()) {
        return;
      }
#endif

      // Force FMOD initialization to avoid a framerate drop when the first
      // sound is played.
      _ = RuntimeManager.StudioSystem;

      var app = Object.Instantiate(Resources.Load<GameObject>("App"));
      Game = app.GetComponent<GameManager>();
      Input = app.GetComponent<InputManager>();
      Audio = app.GetComponent<AudioManager>();
      Save = app.GetComponent<SaveManager>();
      Actions = Input.Actions;

      Game.DrivenAwake();

      Object.DontDestroyOnLoad(app);
    }

#if UNITY_EDITOR
    private static bool ShouldSkip() {
      var activeScene = SceneManager.GetActiveScene();
      var path = activeScene.path;
      foreach (var scene in UnityEditor.EditorBuildSettings.scenes) {
        if (scene.path == path) {
          return false;
        }
      }

      return true;
    }
#endif
  }
}
