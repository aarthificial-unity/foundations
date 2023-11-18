using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Interactions;
using Typewriter;
using UnityEngine;
using Utils;
using Utils.Tweening;

namespace Environment {
  public class Floodlight : MonoBehaviour {
    [SerializeField]
    [EntryFilter(BaseType = typeof(ItemEntry), AllowEmpty = true)]
    private EntryReference _itemFilter;
    [SerializeField]
    [EntryFilter(Variant = EntryVariant.Fact)]
    private EntryReference _turnedOnFact;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private Light[] _lights;

    private TypewriterWatcher _watcher;
    private SpringTween _lightTween;
    private Cached<bool> _turnedOn;
    private float[] _intensities;

    private void Start() {
      Update();
      _lightTween.ForceSet(_turnedOn ? 1 : 0);

      _intensities = new float[_lights.Length];
      for (var i = 0; i < _lights.Length; i++) {
        _intensities[i] = _lights[i].intensity;
        if (!_turnedOn) {
          _lights[i].intensity = 0;
        }
      }
    }

    private void Update() {
      if (_watcher.ShouldUpdate()
        && _turnedOn.HasChanged(_interactable.Context.Get(_itemFilter) == 1)) {
        _interactable.Context.Set(_turnedOnFact, _turnedOn.Value ? 1 : 0);
        _lightTween.Set(_turnedOn ? 1 : 0);
      }
    }

    private void FixedUpdate() {
      if (_lightTween.FixedUpdate(SpringConfig.Slow)) {
        for (var i = 0; i < _lights.Length; i++) {
          _lights[i].intensity = _intensities[i] * _lightTween.X;
        }
      }
    }
  }
}
