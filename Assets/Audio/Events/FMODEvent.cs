using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using JetBrains.Annotations;
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
    private Dictionary<FMODParameter, FMODParameterInstance> _parameterIdLookup;
    public bool IsInitialized { private set; get; }

    public void Setup() {
      if (Event == null) {
        return;
      }

      Instance = RuntimeManager.CreateInstance(Event);

      _parameterIdLookup = new Dictionary<FMODParameter, FMODParameterInstance>();
      foreach (var parameter in Event.Parameters) {
        var parameterInstance = new FMODParameterInstance(parameter, this);
        _parameterIdLookup.Add (parameter, parameterInstance);
      }

      IsInitialized = true;
    }

    public void AttachToTransform (Transform transform) {
      if (transform != null) {
        RuntimeManager.AttachInstanceToGameObject(Instance, transform);
      }
    }

    public void SetParameter(FMODParameter parameter, float val) {
      if (_parameterIdLookup.ContainsKey(parameter)) {
        if (parameter.IsGlobal) {
          RuntimeManager.StudioSystem.setParameterByID(_parameterIdLookup[parameter].ID, val);
          return;
        }
        Instance.setParameterByID(_parameterIdLookup[parameter].ID, val);
      }
    }

    public void Play() {
      Instance.start();
    }

    public void Release() {
      Instance.release();
    }
  }
}
