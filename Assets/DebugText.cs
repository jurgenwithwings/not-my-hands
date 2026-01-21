using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    private TMP_Text debugText;

    private Color defColor;

    public struct DebugTextEntry {
        public float time;
        public object key;
        public TMP_Text text;
        
        public DebugTextEntry(string text, float time, object key, TMP_Text textComponent) {
            this.text = textComponent;
            this.time = time;
            this.key = key;
            this.text.text = text;
        }
    }
    
    private List<DebugTextEntry> debugTextEntries = new List<DebugTextEntry>();
    
    private List<DebugTextEntry> inactiveEntries = new List<DebugTextEntry>();
    
    private void Start() {
        debugText = transform.GetChild(0).GetComponent<TMP_Text>();
        defColor = debugText.color;
        inactiveEntries.Add(new DebugTextEntry("", 0f, null, debugText));
        debugText.gameObject.SetActive(false);
        PlayerHUDEvents.OnDebug += SetDebugText;
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnDebug -= SetDebugText;
    }

    private void SetDebugText(string text, float time, object key, Color? color) {
        text = $"[{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}.{DateTime.Now.Millisecond:000}] {text}";
        if (key != null) {
            for (int i = 0; i < debugTextEntries.Count; i++) {
                DebugTextEntry entry = debugTextEntries[i];
                if (entry.key == key) {
                    entry.text.text = text;
                    entry.text.color = color ?? defColor;
                    entry.time = time;
                    debugTextEntries[i] = entry;
                    return;
                }
            }
        }
        
        if (inactiveEntries.Count > 0) {
            DebugTextEntry entry = inactiveEntries[0];
            inactiveEntries.RemoveAt(0);
            entry.text.text = text;
            entry.text.color = color ?? defColor;
            entry.time = time;
            entry.key = key;
            entry.text.gameObject.SetActive(true);
            entry.text.transform.SetSiblingIndex(transform.childCount - 1);
            debugTextEntries.Add(entry);
            return;
        }
        
        DebugTextEntry newEntry = new(text, time, key, Instantiate(debugText, transform));
        newEntry.text.color = color ?? defColor;
        newEntry.text.transform.SetSiblingIndex(transform.childCount - 1);
        debugTextEntries.Add(newEntry);
    }
    
    private void Update() {
        for (int i = debugTextEntries.Count - 1; i >= 0; i--) { //Reverse for
            DebugTextEntry entry = debugTextEntries[i];
            entry.time -= Time.deltaTime;
            if (entry.time <= 0f) {
                entry.text.gameObject.SetActive(false);
                inactiveEntries.Add(entry);
                debugTextEntries.RemoveAt(i);
            } else {
                debugTextEntries[i] = entry;
            }
        }
    }
}
