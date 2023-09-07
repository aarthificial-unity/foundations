using UnityEngine;

namespace Player {
  public class TargetController : MonoBehaviour {
    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private Transform _bone;

    public Material Material {
      set => _mesh.sharedMaterial = value;
    }

    public float Scale {
      set => _bone.localScale = new Vector3(value, 1, value);
    }

    public bool Visible {
      set => gameObject.SetActive(value);
    }
  }
}
