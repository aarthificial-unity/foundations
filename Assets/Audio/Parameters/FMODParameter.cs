using System;
using FMOD.Studio;
using UnityEngine;

namespace Audio {
  [CreateAssetMenu (fileName = "New FMOD Parameter", menuName = "FMOD Parameter")]
  public class FMODParameter : ScriptableObject {
    public string ParameterName;
    public bool IsGlobal = false;
  }

  [Serializable]
  public class FMODParameterInstance {
    public FMODParameter Parameter;
    private FMODEventInstance _eventInstance;

    public PARAMETER_ID ID { get; private set; }

    public bool IsInitialized { get; private set; }

    public FMODParameterInstance(FMODParameter parameter, FMODEventInstance eventInstance) {
      Parameter = parameter;

      if (Parameter == null || eventInstance == null) return;
      _eventInstance = eventInstance;
      _eventInstance.Instance.getDescription(out var description);
      description.getParameterDescriptionByName(Parameter.ParameterName, out var param);
      ID = param.id;
    }

    public float CurrentValue {
      get {
        if (! IsInitialized) return -1;
        _eventInstance.Instance.getParameterByID (ID, out float value);
        return value;
      }
    }

  }
}
