using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework {
  // TODO Move to the editor namespace maybe?
#if UNITY_EDITOR
  public static class App {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
      if (ShouldBootstrap()) {
        SceneManager.LoadScene(0, LoadSceneMode.Additive);
      }
    }

    private static bool ShouldBootstrap() {
      var activeScene = SceneManager.GetActiveScene();
      if (activeScene.buildIndex == 0) {
        return false;
      }

      var path = activeScene.path;
      foreach (var scene in UnityEditor.EditorBuildSettings.scenes) {
        if (scene.path == path) {
          return true;
        }
      }

      return false;
    }
  }
#endif
}
