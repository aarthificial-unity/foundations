using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio {

  [CreateAssetMenu (fileName = "New FMODEvent", menuName = "FMOD Event")]
  public class FMODEvent : ScriptableObject {
    public EventReference Event;
    public FMODParameter[] Parameters;

    public static implicit operator EventReference(FMODEvent fmodEvent) {
      return fmodEvent.Event;
    }
  }

  [Serializable]
  public class FMODEventInstance {
    public FMODEvent Event;
    public EventInstance Instance;
    private Dictionary<FMODParameter, PARAMETER_ID> _parameterIdLookup;
    public bool IsInitialized { private set; get; }

    public void Setup() {
      if (Event == null) {
        return;
      }

      Instance = RuntimeManager.CreateInstance(Event);

      _parameterIdLookup = new Dictionary<FMODParameter, PARAMETER_ID>();
      Instance.getDescription(out var description);
      foreach (var parameter in Event.Parameters) {
        description.getParameterDescriptionByName(
          parameter.ParameterName,
          out var parameterDescription
        );
        _parameterIdLookup.Add(parameter, parameterDescription.id);
      }

      IsInitialized = true;
    }

    ~FMODEventInstance() {
      Release();
    }

    public void AttachToTransform(Transform transform) {
      if (transform != null) {
        RuntimeManager.AttachInstanceToGameObject(Instance, transform);
      }
    }

    public void SetParameter(FMODParameter parameter, float val) {
      if (parameter.IsGlobal)
        return;

      if (_parameterIdLookup.TryGetValue(parameter, out var instance)) {
        Instance.setParameterByID(instance, val);
      }
    }

    public void Play() {
        Instance.start();
    }

    public void Release() {
      if (IsInitialized) {
        Instance.release();
      }
    }
  }
}
