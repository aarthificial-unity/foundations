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
      yield return SceneManager.LoadSceneAsync(
        _sceneIndex,
        LoadSceneMode.Additive
      );

      SceneManager.SetActiveScene(
        SceneManager.GetSceneByBuildIndex(_sceneIndex)
      );
      _input.SwitchToUI();
    }

    public override IEnumerator OnEnd() {
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
