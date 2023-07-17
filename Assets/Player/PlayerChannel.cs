using Interactions;
using System;
using UnityEngine;

namespace Player {
  public class PlayerChannel : ScriptableObject {
    [NonSerialized] public PlayerManager Manager;
    private PlayerLookup<PlayerController> _players;

    public bool TryGetPlayer(int id, out PlayerController player) {
      if (id == InteractionContext.LT) {
        player = _players.LT;
        return true;
      }

      if (id == InteractionContext.RT) {
        player = _players.RT;
        return true;
      }

      player = null;
      return false;
    }

    public PlayerController LT {
      get => _players.LT;
      set => _players.LT = value;
    }

    public PlayerController RT {
      get => _players.RT;
      set => _players.RT = value;
    }

    public PlayerController this[PlayerType type] {
      get => _players[type];
      set => _players[type] = value;
    }
  }
}
