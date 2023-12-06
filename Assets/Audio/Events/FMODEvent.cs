using Audio.Parameters;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Audio.Events {
  [CreateAssetMenu(fileName = "New FMODEvent", menuName = "FMOD Event")]
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
    private GameObject _positionalGameObject;

    public void Setup() {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        return;
      }
#endif

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

    public void AttachToGameObject(GameObject gameObject) {
      if (gameObject != null) {
        _positionalGameObject = gameObject;
        Update3DPosition();
      }
    }

    public void Update3DPosition() {
      if (_positionalGameObject != null) {
        Instance.set3DAttributes(_positionalGameObject.To3DAttributes());
      }
    }

    public void SetParameter(FMODParameter parameter, float val) {
      if (!IsInitialized || parameter.IsGlobal) {
        return;
      }

      if (_parameterIdLookup.TryGetValue(parameter, out var instance)) {
        Instance.setParameterByID(instance, val);
      }
    }

    public void Play() {
      if (IsInitialized) {
        Instance.start();
      }
    }

    public void Pause() {
      if (IsInitialized) {
        Instance.stop(STOP_MODE.ALLOWFADEOUT);
      }
    }

    public void Release() {
      if (IsInitialized) {
        Instance.getPlaybackState(out var state);
        if (state == PLAYBACK_STATE.PLAYING) {
          Instance.stop(STOP_MODE.IMMEDIATE);
        }

        Instance.release();
        IsInitialized = false;
      }
    }
  }
}
