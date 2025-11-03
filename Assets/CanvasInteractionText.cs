using TMPro;
using UnityEngine;

public class CanvasInteractionText : MonoBehaviour {
    private TMP_Text text;


    private void Awake() {
        text = GetComponent<TMP_Text>();
    }

    private void Start() {
        PlayerHUDEvents.OnSetInteractionText += SetText;
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnSetInteractionText -= SetText;
    }

    private void Update() {
        text.text = "";
    }

    private void SetText(string action) {
        text.text = $"Press E {action}";
    }
}
