using System.Linq;
using TMPro;
using UnityEngine;

public class CanvasInteractionText : MonoBehaviour {
    [SerializeField] private TMP_Text text;

    private CachedGlyph interact = new ("Player/Interact");
    private CachedGlyph primaryInteract = new ("Player/PrimaryInteract");
    private CachedGlyph secondaryInteract = new ("Player/SecondaryInteract");
    
    private void Start() {
        PlayerHUDEvents.OnSetInteractionText += SetText;
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnSetInteractionText -= SetText;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SetText(string promptText, bool isDualAction) {
        if (string.IsNullOrEmpty(promptText)) {
            transform.localScale = Vector3.zero;
            return;
        }
        
        transform.localScale = Vector3.one;

        if (isDualAction) {
            text.text = $"{primaryInteract.Glyph()} or {secondaryInteract.Glyph(false)} {promptText}";
        }
        else {
            text.text = $"{interact.Glyph()} {promptText}";
        }
    }
    
    
}
