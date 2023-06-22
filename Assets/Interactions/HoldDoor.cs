using UnityEngine;

namespace Interactions {
  public class HoldDoor : Interactable {
    [SerializeField] private Interactable _doorLever;

    public override bool CanInteract() {
      return _doorLever.IsInteracting || IsInteracting;
    }
  }
}
