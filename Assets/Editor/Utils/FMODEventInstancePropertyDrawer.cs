using Audio;
using UnityEditor;

namespace Editor.Utils {
  [CustomPropertyDrawer(typeof(FMODEventInstance))]
  public class FMODEventInstancePropertyDrawer : WrapperPropertyDrawer {
    protected override string WrappedName => nameof(FMODEventInstance.Event);
  }
}
