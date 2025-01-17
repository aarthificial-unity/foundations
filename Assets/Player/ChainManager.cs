﻿using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Attributes;
using Aarthificial.Safekeeper.Stores;
using Audio.Events;
using Audio.Parameters;
using Framework;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Tweening;

namespace Player {
  [DefaultExecutionOrder(2)]
  public class ChainManager : MonoBehaviour, ISaveStore {
    private class StoredData {
      public Vector3[] Positions;
      public Quaternion[] Rotations;
    }

    [SerializeField] private GameObject _link;
    [SerializeField] private int _length;
    [SerializeField] private float _linkLength = 1;
    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Quaternion[] _rotations;
    [SerializeField] private FMODEventInstance _chainEvent;
    [SerializeField] private FMODParameter _chainVelocityParam;
    [SerializeField] private FMODParameter _chainAccelerationParam;
    [SerializeField] private SpringConfig _velocitySpringConfig = new(70, 8);
    [SerializeField]
    [FormerlySerializedAs("_accelartionSpringConfig")]
    private SpringConfig _accelerationSpringConfig = new(106, 3);

    [ObjectLocation] [SerializeField] private SaveLocation _id;
    [SerializeField] private PlayerLookup<Rigidbody> _players;

    private Rigidbody[] _links;
    private Vector3 _prevFrameVelocity;
    private float _currentVelocity;
    private float _currentAcceleration;
    private SpringTween _velocityTween;
    private SpringTween _accelerationTween;
    private StoredData _storedData;

    private void Awake() {
#if UNITY_EDITOR
      if (_positions.Length < _length) {
        _positions = new Vector3[_length];
      }

      if (_rotations.Length < _length) {
        _rotations = new Quaternion[_length];
      }
#endif

      _storedData = new StoredData {
        Positions = _positions,
        Rotations = _rotations,
      };

      var from = _players.LT;
      var to = _players.RT;

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

    private void OnDestroy() {
      _chainEvent.Release();
    }

    private void OnEnable() {
      SaveStoreRegistry.Register(this);
    }

    private void OnDisable() {
      SaveStoreRegistry.Unregister(this);
    }

    public void OnLoad(SaveControllerBase save) {
      save.Data.Read(_id, _storedData);
    }

    public void OnSave(SaveControllerBase save) {
      for (var i = 0; i < _length; i++) {
        _storedData.Positions[i] = _links[i].position;
        _storedData.Rotations[i] = _links[i].rotation;
      }
      save.Data.Write(_id, _storedData);
    }

    private void Start() {
      for (var i = 0; i < _length; i++) {
        _links[i].position = _storedData.Positions[i];
        _links[i].rotation = _storedData.Rotations[i];
        _links[i].velocity = Vector3.zero;
      }
      _prevFrameVelocity = Vector3.zero;
      _chainEvent.Setup();
      _chainEvent.AttachToGameObject(_links[_length / 2].gameObject);
      _chainEvent.SetParameter(_chainAccelerationParam, 0.0f);
      _chainEvent.SetParameter(_chainVelocityParam, 0.0f);
      _chainEvent.Play();
    }

    private void FixedUpdate() {
      var currentVelocity = Vector3.zero;
      for (var i = 0; i < _length; i++) {
        currentVelocity += _links[i].velocity;
      }
      _velocityTween.Set(currentVelocity.magnitude);
      _velocityTween.FixedUpdate(_velocitySpringConfig);
      _currentVelocity = _velocityTween.X;
      _accelerationTween.Set((currentVelocity - _prevFrameVelocity).magnitude);
      _accelerationTween.FixedUpdate(_accelerationSpringConfig);
      _currentAcceleration = _accelerationTween.X;
      _prevFrameVelocity = currentVelocity;
    }

    private void Update() {
      _chainEvent.Update3DPosition();
      _chainEvent.SetParameter(
        _chainVelocityParam,
        App.Game.Story.IsPaused ? 0 : _currentVelocity
      );
      _chainEvent.SetParameter(
        _chainAccelerationParam,
        App.Game.Story.IsPaused ? 0 : _currentAcceleration
      );
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
