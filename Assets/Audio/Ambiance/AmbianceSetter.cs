using Framework;
using UnityEngine;

namespace Audio.Ambiance {
  public class AmbianceSetter : MonoBehaviour {
    [SerializeField] private AmbianceType _ambianceType;

    private void Awake() {
      App.Audio.SetAmbiance(_ambianceType);
    }
  }
}
