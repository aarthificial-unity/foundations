using UnityEngine;

namespace View {
  public class Backdrop : MonoBehaviour {
    [SerializeField] private float _speed;
    private CanvasGroup _group;
    private int _requests;

    private void Awake() {
      _group = GetComponent<CanvasGroup>();
      _group.alpha = 1;
      _group.blocksRaycasts = true;
    }

    public bool IsReady() {
      return _requests > 0 ? _group.alpha >= 1 : _group.alpha <= 0;
    }

    public void Request() {
      _requests++;
    }

    public void Release() {
      _requests--;
    }

    private void Update() {
      var direction = _requests > 0 ? 1 : -1;
      _group.alpha = Mathf.Clamp01(
        _group.alpha + Time.unscaledDeltaTime * _speed * direction
      );
      _group.blocksRaycasts = _requests > 0;
    }
  }
}
