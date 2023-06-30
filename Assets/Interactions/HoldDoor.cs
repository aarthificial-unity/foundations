using Items;
using Player;
using UnityEngine;

namespace Interactions {
  public class HoldDoor : Interactable {
    [SerializeField] private Interactable _doorLever;
    [SerializeField] private Item _item;

    // public override bool CanInteract() {
      // return _doorLever.IsInteracting || IsInteracting;
    // }

    // public override bool IsCompatibleWith(PlayerController player) {
      // return base.IsCompatibleWith(player) && player.HasItem(_item);
    // }
  }
}
