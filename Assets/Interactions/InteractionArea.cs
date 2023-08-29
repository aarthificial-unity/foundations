using Player;
using System;
using UnityEngine;

namespace Interactions {
  public class InteractionArea : MonoBehaviour {
    [NonSerialized] public PlayerLookup<bool> IsPlayerInside;

    private void OnTriggerEnter(Collider other) {
      if (other.gameObject.TryGetComponent(out PlayerController player)) {
        IsPlayerInside[player.Type] = true;
      }
    }

    private void OnTriggerExit(Collider other) {
      if (other.gameObject.TryGetComponent(out PlayerController player)) {
        IsPlayerInside[player.Type] = false;
      }
    }
  }
}
