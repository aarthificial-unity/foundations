using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace View.Credits {
  public class CreditsListView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private CreditsListData _data;
    [SerializeField] private RectTransform _view;
    [SerializeField] private RectTransform _content;
    [SerializeField] private float _scrollSpeed = 50f;
    [SerializeField] private float _entryHeight = 50f;
    [SerializeField] private int _maxEntries = 10;
    [SerializeField] private TextMeshProUGUI _entryPrefab;

    private int _entryCount;
    private TextMeshProUGUI[] _entries;
    private float _scrollOffset;
    private Cached<int> _indexOffset;
    private bool _isMouseDown;

    private void Awake() {
      _entryCount = Mathf.Min(_data.Entries.Length, _maxEntries) + 1;
      _entries = new TextMeshProUGUI[_entryCount];
      var layout = _view.GetComponent<LayoutElement>();
      layout.minHeight = (_entryCount - 1) * _entryHeight;
      _content.sizeDelta = new Vector2(0, _entryCount * _entryHeight);

      for (var i = 0; i < _entryCount; i++) {
        var entry = Instantiate(_entryPrefab, _content);
        entry.text = _data.Entries[i % _data.Entries.Length];
        entry.rectTransform.sizeDelta = new Vector2(0, _entryHeight);
        entry.rectTransform.anchoredPosition =
          new Vector2(0, -i * _entryHeight);
        _entries[i] = entry;
      }
    }

    private void Update() {
      if (_entryCount > _data.Entries.Length) {
        return;
      }

      _scrollOffset += Time.deltaTime * (_isMouseDown ? 20 : _scrollSpeed);
      _content.anchoredPosition = new Vector2(0, _scrollOffset % _entryHeight);
      if (_indexOffset.HasChanged(
          Mathf.FloorToInt(_scrollOffset / _entryHeight)
        )) {
        for (var i = 0; i < _entryCount; i++) {
          _entries[i].text = _data.Entries[
            (_indexOffset + i) % _data.Entries.Length];
        }
      }
    }

    public void OnPointerDown(PointerEventData eventData) {
      _isMouseDown = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
      _isMouseDown = false;
    }
  }
}
