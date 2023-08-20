using UnityEngine;
using UnityEngine.UI;

namespace View {
  public class BoxSDF : BaseMeshEffect {
    [SerializeField] private float _padding;
    [SerializeField] private float _strokeWidth = 0.1f;
    [SerializeField] private bool _filled;
    private RectTransform _rectTransform;

    protected override void Awake() {
      base.Awake();
      _rectTransform = GetComponent<RectTransform>();
    }

#if UNITY_EDITOR
    private void Update() {
      if (TryGetComponent(out Image mask)) {
        mask.RecalculateMasking();
      }
    }
#endif

    public override void ModifyMesh(VertexHelper vh) {
      if (!IsActive() || vh.currentVertCount == 0)
        return;

      var vert = new UIVertex();
      var rect = _rectTransform.rect;
      for (var i = 0; i < vh.currentVertCount; i++) {
        vh.PopulateUIVertex(ref vert, i);
        vert.uv1 = new Vector4(
          rect.size.x,
          rect.size.y,
          _padding,
          _filled ? 100000 : _strokeWidth
        );
        vh.SetUIVertex(vert, i);
      }
    }
  }
}
