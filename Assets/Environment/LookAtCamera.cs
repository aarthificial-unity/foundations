using UnityEngine;
using Utils;
using View.Overlay;

namespace Environment {
  [DefaultExecutionOrder(1000)]
  public class LookAtCamera : MonoBehaviour {
    [Inject] [SerializeField] private OverlayChannel _overlay;

    private void LateUpdate() {
      if (_overlay.IsReady) {
        transform.rotation =
          _overlay.CameraManager.MainCamera.transform.rotation;
      }
    }
  }
}
