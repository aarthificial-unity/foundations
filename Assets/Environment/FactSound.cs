using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Tools;
using Audio.Events;
using Framework;
using UnityEngine;
using Utils;

namespace Environment {
  public class FactSound : MonoBehaviour {
    [SerializeField] private TypewriterCriteria _criteria;
    [SerializeField] private FMODEventInstance _sound;

    private TypewriterWatcher _watcher;
    private Cached<bool> _matches;

    private void Awake() {
      _sound.Setup();
    }

    private void OnDestroy() {
      _sound.Release();
    }

    private void Start() {
      _matches = App.Game.Story.Test(_criteria);
    }

    private void Update() {
      if (_watcher.ShouldUpdate()
        && _matches.HasChanged(App.Game.Story.Test(_criteria))
        && _matches) {
        _sound.Play();
      }
    }
  }
}
