using Framework;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View {
  public class MenuView : MonoBehaviour {
    [SerializeField] [Inject] private StoryMode _storyMode;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _settingsButton;

    private void OnEnable() {
      _startButton.onClick.AddListener(_storyMode.RequestStart);
      _exitButton.onClick.AddListener(_storyMode.Quit);
      _startButton.Select();
    }
  }
}
