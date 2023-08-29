using UnityEngine;
using UnityEngine.UI;

namespace View {
  public class BoxSDF : BaseMeshEffect {
    [SerializeField] private float _padding;
    [SerializeField] private float _strokeWidth = 1;
    [SerializeField] private bool _filled;
    [SerializeField] private float _textureStrength;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private bool _hasCanvas;

    protected override void Awake() {
      base.Awake();
      _rectTransform = GetComponent<RectTransform>();
      _canvas = GetComponentInParent<Canvas>();
      _hasCanvas = _canvas != null;
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

      var corner = _rectTransform.TransformPoint(_rectTransform.rect.position);
      var position = _hasCanvas
        ? _canvas.transform.InverseTransformPoint(corner)
        + new Vector3(
          _canvas.GetInstanceID() % 453,
          _canvas.GetInstanceID() % 5632,
          0
        )
        : Vector3.zero;

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
        vert.uv2 = new Vector4(position.x, position.y, _textureStrength);
        vh.SetUIVertex(vert, i);
      }
    }
  }
}
