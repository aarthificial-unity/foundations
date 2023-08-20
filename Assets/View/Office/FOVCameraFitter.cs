using Cinemachine;
using UnityEngine;
using Utils;

namespace View.Office {
  public class FOVCameraFitter : MonoBehaviour {
    [SerializeField] private float _ratio;
    private CinemachineVirtualCamera _camera;
    private CameraFitter _fitter;

    private void Awake() {
      _camera = GetComponent<CinemachineVirtualCamera>();
      _fitter = new CameraFitter(_ratio, _camera.m_Lens.FieldOfView);
    }

    private void Update() {
      _fitter.Update(ref _camera.m_Lens.FieldOfView);
    }
  }
}
