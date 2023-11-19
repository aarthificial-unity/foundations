using Audio.Events;
using UnityEngine;

namespace Audio {
  public class AudioEmitter : MonoBehaviour {
    [SerializeField] private FMODEventInstance _sound;
    [SerializeField] private bool _playOnEnable;

    private void Awake() {
      _sound.Setup();
      _sound.AttachToGameObject(gameObject);
    }

    private void OnEnable() {
      if (_playOnEnable) {
        _sound.Play();
      }
    }

    private void Update() {
      if (!gameObject.isStatic) {
        _sound.Update3DPosition();
      }
    }

    private void OnDisable() {
      _sound.Pause();
    }

    private void OnDestroy() {
      _sound.Release();
    }
  }
}
