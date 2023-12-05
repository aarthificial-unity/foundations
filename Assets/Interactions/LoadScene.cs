using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using UnityEngine;
using Utils;
using View.Overlay;

namespace Interactions {
  [RequireComponent(typeof(Interactable))]
  public class LoadScene : MonoBehaviour {
    [SerializeField] [ScenePath] private string _scenePath;
    [SerializeField]
    [EntryFilter(Variant = EntryVariant.Event)]
    private EntryReference _event;
    private Interactable _interactable;
    private OverlayManager _overlay;

    private void Awake() {
      _overlay = FindObjectOfType<OverlayManager>();
      _interactable = GetComponent<Interactable>();
      TypewriterDatabase.Instance.AddListener(_event, HandleTypewriterEvent);
    }

    private void OnDestroy() {
      TypewriterDatabase.Instance.RemoveListener(_event, HandleTypewriterEvent);
    }

    private void HandleTypewriterEvent(
      BaseEntry entry,
      ITypewriterContext context
    ) {
      if (_interactable.Context == context) {
        _overlay.SwapState.Enter(_scenePath);
      }
    }
  }
}
