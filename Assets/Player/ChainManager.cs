using UnityEngine;

namespace Player {
  [DefaultExecutionOrder(2)]
  public class ChainManager : MonoBehaviour {
    [SerializeField] private GameObject _link;
    [SerializeField] private int _length;
    [SerializeField] private float _linkLength = 1;

    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Quaternion[] _rotations;

    private Rigidbody[] _links;

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
    }

    private void Start() {
      for (var i = 0; i < _length; i++) {
        _links[i].position = _positions[i];
        _links[i].rotation = _rotations[i];
        _links[i].velocity = Vector3.zero;
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
