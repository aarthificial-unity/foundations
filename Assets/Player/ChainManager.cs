using Audio;
using Player.Surfaces;
using UnityEngine;
using Utils;
using Utils.Tweening;

namespace Player {
  [DefaultExecutionOrder(2)]
  public class ChainManager : MonoBehaviour {
    [SerializeField] private GameObject _link;
    [SerializeField] private int _length;
    [SerializeField] private float _linkLength = 1;

    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Quaternion[] _rotations;

    [SerializeField] private FMODEventInstance _chainEvent;
    [SerializeField] private FMODParameter _chainVelocityParam, _chainAccelerationParam, _chainSurfaceParam;

    private Rigidbody[] _links;
    private Rigidbody _middleLink;
    private Vector3 _prevFrameVelocity;

    private SpringTween _velocityTween;
    private SpringTween _accelTween;
    [SerializeField] private SpringConfig _velocitySpringConfig;
    [SerializeField] private SpringConfig _accelSpringConfig;
    [Inject] public PlayerConfig Config;

    private void Awake() {
#if UNITY_EDITOR
      if (_positions.Length < _length) {
        _positions = new Vector3[_length];
      }

      if (_rotations.Length < _length) {
        _rotations = new Quaternion[_length];
      }
#endif

      var players = GetComponent<PlayerManager>();
      var from = players.LT.ChainTarget;
      var to = players.RT.ChainTarget;

      _links = new Rigidbody[_length];
      var prev = Instantiate(_link);
      _links[0] = prev.GetComponent<Rigidbody>();

      var joint = to.GetComponent<Joint>();
      joint.connectedBody = _links[0];
      var anchor = joint.connectedAnchor;
      anchor.y = -_linkLength;
      joint.connectedAnchor = anchor;

      for (var i = 1; i < _length; i++) {
        var link = Instantiate(_link);
        joint = prev.GetComponent<Joint>();
        _links[i] = link.GetComponent<Rigidbody>();
        joint.connectedBody = _links[i];
        anchor = joint.connectedAnchor;
        anchor.y = -_linkLength;
        joint.connectedAnchor = anchor;
        anchor = joint.anchor;
        anchor.y = _linkLength;
        joint.anchor = anchor;

        if (i % 2 == 0) {
          joint.transform.GetChild(0).Rotate(0, 90, 0);
        }

        prev = link;
      }

      Destroy(prev.GetComponent<Joint>());
      joint = from.GetComponent<Joint>();
      joint.connectedBody = prev.GetComponent<Rigidbody>();
      anchor = joint.connectedAnchor;
      anchor.y = _linkLength;
      joint.connectedAnchor = anchor;

      _middleLink = _links[_length / 2];
    }

    private void Start() {
      for (var i = 0; i < _length; i++) {
        _links[i].position = _positions[i];
        _links[i].rotation = _rotations[i];
        _links[i].velocity = Vector3.zero;
      }
      _prevFrameVelocity = Vector3.zero;
      _chainEvent.Setup();
      _chainEvent.AttachToGameObject(_middleLink.gameObject);
      _chainEvent.SetParameter(_chainVelocityParam, 0.0f);
      _chainEvent.SetParameter(_chainAccelerationParam, 0.0f);
      _chainEvent.Play();
    }

    private void FixedUpdate() {
      Vector3 currentVelocity = Vector3.zero;
      for (var i = 0; i < _length; i++) {
        currentVelocity += _links[i].velocity;
      }
      Vector3 accel = currentVelocity - _prevFrameVelocity;
      _prevFrameVelocity = currentVelocity;
      _velocityTween.Set(currentVelocity.magnitude);
      _velocityTween.FixedUpdate(_velocitySpringConfig);
      _accelTween.Set(accel.magnitude);
      _accelTween.FixedUpdate(_accelSpringConfig);
      _chainEvent.Update3DPosition();
      _chainEvent.SetParameter(_chainVelocityParam, _velocityTween.X);
      _chainEvent.SetParameter(_chainAccelerationParam, _accelTween.X);
      if (Physics.Raycast(
          _middleLink.position,
          Vector3.down,
          out var hit,
          2f,
          Config.GroundMask
        )
        && hit.collider.TryGetComponent<ISurfaceProvider>(out var surface)) {
        _chainEvent.SetParameter(_chainSurfaceParam, surface.GetSurface(hit));
      }
    }

#if UNITY_EDITOR
    [ContextMenu("Store Chain Transform")]
    private void StoreChainTransform() {
      _positions = new Vector3[_length];
      _rotations = new Quaternion[_length];
      for (var i = 0; i < _length; i++) {
        var link = _links[i].transform;
        _positions[i] = link.position;
        _rotations[i] = link.rotation;
      }
    }
#endif
  }
}
