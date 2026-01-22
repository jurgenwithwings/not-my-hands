using System.Linq;
using TMPro;
using UnityEngine;

public class CanvasInteractionText : MonoBehaviour {
    [SerializeField] private TMP_Text text;

    private CachedGlyph interact = new ("Player/Interact");
    private CachedGlyph primaryInteract = new ("Player/PrimaryInteract");
    private CachedGlyph secondaryInteract = new ("Player/SecondaryInteract");

    private void Awake() {
        text = GetComponent<TMP_Text>();
    }
    
    private void Start() {
        PlayerHUDEvents.OnSetInteractionText += SetText;
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnSetInteractionText -= SetText;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SetText(string promptText, bool isDualAction) {
        if (string.IsNullOrEmpty(promptText)) {
            text.text = "";
            return;
        }

        if (isDualAction) {
            text.text = $"Press {primaryInteract.Glyph()} or {secondaryInteract.Glyph(false)} {promptText}";
        }
        else {
            text.text = $"Press {interact.Glyph()} {promptText}";
        }
    }
    
    
}
