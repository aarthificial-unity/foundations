using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace View {
  public class BoxSDF : BaseMeshEffect {
    [FormerlySerializedAs("Padding")]
    [SerializeField]
    private float _padding;
    [FormerlySerializedAs("StrokeWidth")]
    [SerializeField]
    private float _strokeWidth = 1;
    [FormerlySerializedAs("Filled")]
    [SerializeField]
    private bool _filled;
    [FormerlySerializedAs("TextureStrength")]
    [SerializeField]
    private float _textureStrength;
    [SerializeField] private float _dash;
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private bool _hasCanvas;
    private Graphic _graphic;

    public Color Color {
      get => _graphic.color;
      set => _graphic.color = value;
    }

    public float Padding {
      set {
        if (_padding != value) {
          _padding = value;
          _graphic.SetVerticesDirty();
        }
      }
    }

    public float StrokeWidth {
      set {
        if (_strokeWidth != value) {
          _strokeWidth = value;
          _graphic.SetVerticesDirty();
        }
      }
    }

    public bool Filled {
      set {
        if (_filled != value) {
          _filled = value;
          _graphic.SetVerticesDirty();
        }
      }
    }

    public float TextureStrength {
      set {
        if (_textureStrength != value) {
          _textureStrength = value;
          _graphic.SetVerticesDirty();
        }
      }
    }

    public float Dash {
      set {
        if (_dash != value) {
          _dash = value;
          _graphic.SetVerticesDirty();
        }
      }
    }

    protected override void Awake() {
      base.Awake();
      _rectTransform = GetComponent<RectTransform>();
      _canvas = GetComponentInParent<Canvas>();
      _graphic = GetComponent<Graphic>();
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
        vert.uv2 = new Vector4(position.x, position.y, _textureStrength, _dash);
        vh.SetUIVertex(vert, i);
      }
    }
  }
}
