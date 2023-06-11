using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework {
  public static class App {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
      if (SceneManager.GetActiveScene().buildIndex != 0) {
        SceneManager.LoadScene(0, LoadSceneMode.Additive);
      }
    }
  }
}
