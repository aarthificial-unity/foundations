using Player;

namespace Utils {
  public static class PlayerTypeExtensions {
    public static PlayerType Other(this PlayerType type) {
      return type switch {
        PlayerType.Both => PlayerType.Both,
        PlayerType.LT => PlayerType.RT,
        PlayerType.RT => PlayerType.LT,
        _ => PlayerType.None,
      };
    }
  }
}
