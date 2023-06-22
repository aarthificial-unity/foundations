using FMODUnity;
using Framework;
using UnityEngine;
using Utils;

namespace View {
  public class MenuView : MonoBehaviour {
    [SerializeField] [Inject] private StoryMode _storyMode;
    [SerializeField] private ButtonNavigation _startButton;
    [SerializeField] private ButtonNavigation _exitButton;
    [SerializeField] private ButtonNavigation _settingsButton;

    private void OnEnable() {
      _startButton.Button.onClick.AddListener(_storyMode.RequestStart);
      _exitButton.Button.onClick.AddListener(_storyMode.Quit);
      _startButton.QuickSelect();
    }
  }
}
