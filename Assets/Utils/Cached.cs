using System.Runtime.CompilerServices;

namespace Utils {
  public struct Cached<T> {
    public T Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasChanged(T newValue) {
      if (Value?.Equals(newValue) ?? newValue == null) {
        return false;
      }

      Value = newValue;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(Cached<T> cached) {
      return cached.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Cached<T>(T value) {
      return new Cached<T> { Value = value };
    }
  }
}
