using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Interactions;
using Typewriter;
using UnityEngine;
using Utils;

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
    private Cached<bool> _turnedOn;

    private void Update() {
      if (!_watcher.ShouldUpdate()
        || !_turnedOn.HasChanged(_interactable.Context.Get(_itemFilter) == 1)) {
        return;
      }

      _interactable.Context.Set(_turnedOnFact, _turnedOn.Value ? 1 : 0);
      if (_turnedOn) {
        foreach (var lightItem in _lights) {
          lightItem.enabled = true;
        }
      } else {
        foreach (var lightItem in _lights) {
          lightItem.enabled = false;
        }
      }
    }
  }
}
