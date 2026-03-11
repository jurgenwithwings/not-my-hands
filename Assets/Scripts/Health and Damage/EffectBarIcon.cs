using System;
using ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectBarIcon : MonoBehaviour, IPoolable<EffectBarIcon> {
    // Object Pool
    public Action<EffectBarIcon> ReturnToPool { get; set; }
    public string ObjectPoolKey() => "EffectBarIcon";
    public void OnPoolPull() { }

    public void OnPoolPush() {
        effect = null;
        image.enabled = false;
        text.enabled = false;
        UnSubscribeEvents();
    }

    private void OnDestroy() {
        UnSubscribeEvents();
    }


    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;
    
    private StatusEffect effect;
    
    public void Init(StatusEffect statusEffect) {
        effect = statusEffect;
        
        effect.OnAddStack += SetCountText;
        effect.OnRemoveStack += SetCountText;
        effect.OnRemoveEffect += Remove;
        
        image.enabled = true;
        text.enabled = true;
        
        image.sprite = effect.Data.icon;
        image.color = Color.white;
        SetCountText(effect.Stacks);
        text.color = Color.white;
    }

    private void UnSubscribeEvents() {
        if (effect != null) {
            effect.OnAddStack -= SetCountText;
            effect.OnRemoveStack -= SetCountText;
            effect.OnRemoveEffect -= Remove;
        }
    }

    private void Remove() {
        ReturnToPool?.Invoke(this);
    }

    public void SetCountText(int stacks) {
        text.text = "x" + stacks;
    }

    Color clearColor = new(0.9f, 0.15f, 0.15f, 0.7f);
    private void Update() {
        Color color = Color.Lerp(clearColor, Color.white, effect.NormalizedDuration);
        image.color = color;
        text.color = color;
    }
}