using UnityEngine;
using UnityEngine.EventSystems;

namespace View.Dialogue {
  public class DialogueScrollViewport : MonoBehaviour, IDragHandler {
    public void OnDrag(PointerEventData eventData) {
      eventData.Use();
    }
  }
}
