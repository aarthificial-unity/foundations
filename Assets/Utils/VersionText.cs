using TMPro;
using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionText : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<TextMeshProUGUI>().text = "Version: " + Application.version;
        }
    }
}