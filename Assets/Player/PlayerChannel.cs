using System;
using UnityEngine;

namespace Player {
  public class PlayerChannel : ScriptableObject {
    [NonSerialized] public PlayerManager Manager;
    private PlayerLookup<PlayerController> _players;

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
