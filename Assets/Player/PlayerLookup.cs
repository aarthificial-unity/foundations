using System;

namespace Player {
  [Serializable]
  public struct PlayerLookup<T> {
    public T LT;
    public T RT;

    public T this[PlayerType type] {
      get =>
        type switch {
          PlayerType.LT => LT,
          PlayerType.RT => RT,
          _ => default,
        };
      set {
        switch (type) {
          case PlayerType.LT:
            LT = value;
            break;
          case PlayerType.RT:
            RT = value;
            break;
        }
      }
    }
  }
}
