using System;
using System.Collections;
using UnityEngine;

namespace Player {
  public class ChainManager : MonoBehaviour {
    [SerializeField] private GameObject _link;
    [SerializeField] private int _length;
    [SerializeField] private float _linkLength = 1;

    [SerializeField] private Rigidbody _from;
    [SerializeField] private Rigidbody _to;
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

      _links = new Rigidbody[_length];
      var prev = Instantiate(_link);
      prev.transform.position = _positions[0];
      prev.transform.rotation = _rotations[0];
      _links[0] = prev.GetComponent<Rigidbody>();

      var joint = _to.GetComponent<Joint>();
      joint.connectedBody = _links[0];
      var anchor = joint.connectedAnchor;
      anchor.y = -_linkLength;
      joint.connectedAnchor = anchor;
      anchor = joint.anchor;
      anchor.y = _linkLength;
      joint.anchor = anchor;

      for (var i = 1; i < _length; i++) {
        var link = Instantiate(_link);
        link.transform.position = _positions[i];
        link.transform.rotation = _rotations[i];
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
      joint = _from.GetComponent<Joint>();
      joint.connectedBody = prev.GetComponent<Rigidbody>();
      anchor = joint.connectedAnchor;
      anchor.y = _linkLength;
      joint.connectedAnchor = anchor;
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
