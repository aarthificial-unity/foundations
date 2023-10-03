using System;
using System.Text;
using UnityEngine;

namespace View.Controls {
  public class ExternalLink : MonoBehaviour {
    [SerializeField] private string _url;
    [SerializeField] private bool _scramble;

    private PaperButton _button;
    private string _realUrl;

    private void Awake() {
      _realUrl = _scramble
        ? Encoding.UTF8.GetString(Convert.FromBase64String(_url))
        : _url;

      _button = GetComponent<PaperButton>();
      _button.Clicked += () => Application.OpenURL(_realUrl);
    }
  }
}
