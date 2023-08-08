using UnityEngine;

namespace Environment {
  [ExecuteAlways]
  public class UtilityPole : MonoBehaviour {
    [SerializeField] private GameObject _cable;
    [SerializeField] private GameObject[] _cables = new GameObject[4];
    [SerializeField] private UtilityPole _target;
    [SerializeField] private Transform _tip;

    private void OnEnable() {
      UpdatePosition();
    }

#if UNITY_EDITOR
    private void Update() {
      UpdatePosition();
    }
#endif

    private void UpdatePosition() {
      if (_target == null || _tip == null || _cable == null) {
        return;
      }

      for (var i = 0; i < _cables.Length; i++) {
        var cable = _cables[i];
        if (cable == null) {
          cable = _cables[i] = Instantiate(_cable, transform);
        }

        var from = _tip.position + _tip.right * (i - 1.5f);
        var to = _target._tip.position + _target._tip.right * (i - 1.5f);
        cable.transform.position = (from + to) / 2f;
        cable.transform.rotation =
          Quaternion.LookRotation(from - to, Vector3.up);
        var scale = cable.transform.localScale;
        scale.z = Vector3.Distance(from, to) / 2f;
        cable.transform.localScale = scale;
      }
    }
  }
}
