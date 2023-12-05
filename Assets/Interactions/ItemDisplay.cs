using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Items;
using Typewriter;
using UnityEngine;

namespace Interactions {
  public class ItemDisplay : MonoBehaviour {
    [SerializeField]
    [EntryFilter(BaseType = typeof(ItemEntry), AllowEmpty = true)]
    private EntryReference _itemFilter;
    [SerializeField] private Interactable _interactable;

    private Item _itemInstance;
    private TypewriterWatcher _watcher;

    private void Awake() {
      _itemInstance = Instantiate(
        _itemFilter.GetEntry<ItemEntry>().Prefab,
        transform
      );
    }

    private void Update() {
      if (_watcher.ShouldUpdate()) {
        _itemInstance.gameObject.SetActive(
          _interactable.Context.Get(_itemFilter) == 1
        );
      }
    }
  }
}
