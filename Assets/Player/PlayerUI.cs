using UnityEngine;
using View.Dialogue;

namespace Player {
  [DefaultExecutionOrder(300)]
  public class PlayerUI : MonoBehaviour {
    private PlayerManager _players;
    private DialogueView _dialogue;

    private void Awake() {
      _players = GetComponent<PlayerManager>();
      _dialogue = FindObjectOfType<DialogueView>();
    }

    private void LateUpdate() {
      _dialogue.DrivenUpdate(
        _players.LT.transform.position,
        _players.RT.transform.position
      );
    }
  }
}
