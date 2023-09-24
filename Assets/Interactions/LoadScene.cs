using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;
using UnityEngine;
using Utils;
using View.Overlay;

namespace Interactions {
  [RequireComponent(typeof(Conversation))]
  public class LoadScene : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;
    [SerializeField] [ScenePath] private string _scenePath;
    [SerializeField]
    [EntryFilter(Variant = EntryVariant.Event)]
    private EntryReference _event;
    private Conversation _conversation;

    private void Awake() {
      _conversation = GetComponent<Conversation>();
      TypewriterDatabase.Instance.AddListener(_event, HandleTypewriterEvent);
    }

    private void OnDestroy() {
      TypewriterDatabase.Instance.RemoveListener(_event, HandleTypewriterEvent);
    }

    private void HandleTypewriterEvent(
      BaseEntry entry,
      ITypewriterContext context
    ) {
      if (_conversation.Context == context) {
        _overlay.Manager.SwapState.Enter(_scenePath);
      }
    }
  }
}
