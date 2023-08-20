using UnityEngine;
using Utils;

namespace View.Settings {
  public class SettingsCamera : MonoBehaviour {
    [SerializeField] private float _ratio;
    private Camera _camera;
    private CameraFitter _fitter;

    private void Awake() {
      _camera = GetComponent<Camera>();
      _fitter = new CameraFitter(_ratio, _camera.fieldOfView);
    }

    private void Update() {
      if (_fitter.TryUpdate(out var fov)) {
        _camera.fieldOfView = fov;
      }
    }
  }
}
