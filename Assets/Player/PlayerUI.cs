using UnityEngine;
using Utils;
using View.Overlay;

namespace Player {
  [DefaultExecutionOrder(300)]
  public class PlayerUI : MonoBehaviour {
    [Inject] [SerializeField] private PlayerChannel _players;
    [Inject] [SerializeField] private OverlayChannel _overlay;

    private void LateUpdate() {
      var lt = _players.LT.transform.position;
      var rt = _players.RT.transform.position;
      _overlay.Dialogue.DrivenUpdate(lt, rt);
    }
  }
}
