using System.Collections;
using Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Framework {
  public class MenuMode : GameMode {
    private const int _sceneIndex = 1;
    [SerializeField] [Inject] private InputChannel _input;

    public void RequestStart() {
      Manager.RequestMode(this);
    }

    public override IEnumerator OnStart() {
      Time.timeScale = 0;
      yield return SceneManager.LoadSceneAsync(
        _sceneIndex,
        LoadSceneMode.Additive
      );

      SceneManager.SetActiveScene(
        SceneManager.GetSceneByBuildIndex(_sceneIndex)
      );
      Time.timeScale = 1;
      _input.SwitchToUI();
    }

    public override IEnumerator OnEnd() {
      Time.timeScale = 0;
      yield return SceneManager.UnloadSceneAsync(_sceneIndex);
    }

#if UNITY_EDITOR
    public override void OnEditorStart() {
      SceneManager.SetActiveScene(
        SceneManager.GetSceneByBuildIndex(_sceneIndex)
      );
      _input.SwitchToUI();
    }
#endif
  }
}
