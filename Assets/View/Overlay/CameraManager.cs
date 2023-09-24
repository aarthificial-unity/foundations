using UnityEngine;
using Utils;

namespace View.Overlay {
  public class CameraManager : MonoBehaviour {
    [SerializeField] private float _ratio;

    public Camera OverlayCamera;
    private CameraFitter _fitter;

    private void Awake() {
      _fitter = new CameraFitter(_ratio, OverlayCamera.fieldOfView);
    }

    private void Update() {
      if (_fitter.TryUpdate(out var fov)) {
        OverlayCamera.fieldOfView = fov;
      }
    }
  }
}
