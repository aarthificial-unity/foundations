using UnityEditor;
using View;

namespace Editor.Utils {
  [CustomPropertyDrawer(typeof(Backdrop.Handle))]
  public class BackdropHandlePropertyDrawer : WrapperPropertyDrawer {
    protected override string WrappedName => "_backdrop";
  }
}
