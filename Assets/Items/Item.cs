using Interactions;
using Player;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Items {
  public class Item : MonoBehaviour {
    public AssetReference PrefabReference;
    public Item Prefab;
    public Sprite Icon;

    public virtual bool CanDrop() {
      return true;
    }
  }
}
