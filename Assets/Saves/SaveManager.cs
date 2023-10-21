using Aarthificial.Safekeeper.Loaders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saves {
  public class SaveManager : MonoBehaviour {
    [SerializeField] private string _defaultSceneName = "EntryGate";
    private readonly Dictionary<int, WeakReference<SaveController>> _saves =
      new();

    public SaveController GetSaveController(int id) {
      if (!_saves.ContainsKey(id)) {
        _saves[id] = new WeakReference<SaveController>(null, false);
      }

      var reference = _saves[id];
      if (reference.TryGetTarget(out var save)) {
        return save;
      }

      save = new SaveController(
        new FileSaveLoader(id.ToString()),
        _defaultSceneName,
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        id != 2
#else
        true
#endif
      );
      save.Initialize();
      reference.SetTarget(save);

      return save;
    }

    public async Task<SaveController> GetEditorSave() {
      var currentScene = SceneManager.GetActiveScene().name;
      var save = new SaveController(new DummySaveLoader(), currentScene, true);
      save.Initialize();
      await save.Create();
      save.GlobalData.SceneName = currentScene;
      await save.Save();

      return save;
    }
  }
}
