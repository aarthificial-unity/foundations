using System;

namespace Player {
  [Flags]
  public enum PlayerType {
    None = 0,
    LT = 1,
    RT = 2,
    Both = LT | RT,
  }
}
