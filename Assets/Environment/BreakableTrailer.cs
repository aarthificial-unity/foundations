using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Framework;
using UnityEngine;
using Utils;

namespace Environment {
  public class BreakableTrailer : MonoBehaviour {
    [SerializeField]
    [EntryFilter(Variant = EntryVariant.Fact)]
    private EntryReference _trailerFact;

    [SerializeField] private GameObject _falseObject;
    [SerializeField] private GameObject _trueObject;

    private TypewriterWatcher _watcher;
    private Cached<bool> _isBroken;

    private void Start() {
      Update();
    }

    private void Update() {
      if (_watcher.ShouldUpdate()
        && _isBroken.HasChanged(App.Game.Story.Get(_trailerFact) == 1)) {
        _falseObject.SetActive(!_isBroken);
        _trueObject.SetActive(_isBroken);
      }
    }
  }
}
