using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio {
  [CreateAssetMenu (fileName = "New FMOD Parameter", menuName = "FMOD Parameter")]
  public class FMODParameter : ScriptableObject {
    public string ParameterName;
    public bool IsGlobal = false;
  }

  [Serializable]
  public class FMODParameterInstance {
    [SerializeField] private FMODParameter _parameter;
    private PARAMETER_ID _id;

    public float CurrentValue {
      get {
        RuntimeManager.StudioSystem.getParameterByID(_id, out var value);
        return value;
      }
      set => RuntimeManager.StudioSystem.setParameterByID(_id, value);
    }

    public void Setup() {
      RuntimeManager.StudioSystem.getParameterDescriptionByName(
        _parameter.ParameterName,
        out var description
      );
      _id = description.id;
    }

  }
}
