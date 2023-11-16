using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Framework;
using UnityEngine;
using Utils.Tweening;

namespace Environment {
  public class SecurityKiosk : MonoBehaviour {
    [SerializeField]
    [EntryFilter(Variant = EntryVariant.Fact)]
    private EntryReference _doorOpenFact;

    [SerializeField] private Transform _door;
    [SerializeField] private Vector2 _doorAngles;
    private SpringTween _doorTween;
    private TypewriterWatcher _watcher;

    private float CurrentAngle =>
      App.Game.Story.Get(_doorOpenFact) == 0 ? _doorAngles.x : _doorAngles.y;

    private void Start() {
      _doorTween.ForceSet(CurrentAngle);
    }

    private void Update() {
      if (_watcher.ShouldUpdate()) {
        _doorTween.Set(CurrentAngle);
      }
    }

    private void FixedUpdate() {
      if (_doorTween.FixedUpdate(SpringConfig.Slow)) {
        _door.localRotation = Quaternion.Euler(0, _doorTween.X, 0);
      }
    }
  }
}
