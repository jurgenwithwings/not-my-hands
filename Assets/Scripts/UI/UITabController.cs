using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UITab {
    [Header("Required")]
    public Button button;
    [Space]
    [Header("Requires One")]
    public RectTransform rectTransform;
    public ScrollRect scrollRect;
}

public class UITabController : MonoBehaviour {
    [Header("Tabs")] 
    [SerializeField] private RectTransform container;

    [SerializeField] private float tabSpacing = 20f;
    [SerializeField] private Image interactionBlocker;
    [SerializeField] private List<UITab> tabs;
    private float containerWidth => container.rect.size.x + tabSpacing;
    private Coroutine tabCoroutine;

    private void Awake() {
        for (int i = 0; i < tabs.Count; i++) {
            int index = i;
            tabs[i].button.onClick.AddListener(() => OnTabButtonClicked(index));
        }

        StartCoroutine(CorrectTabPosition());
    }

    private IEnumerator CorrectTabPosition() {
        yield return null;
        yield return null;

        SetTabPositions();
    }

    private void SetTabPositions() {
        for (int i = 0; i < tabs.Count; i++) {
            tabs[i].rectTransform.anchoredPosition = containerWidth * i * Vector2.right;
        }
    }

    private void OnTabButtonClicked(int index) {
        if (tabCoroutine != null) {
            StopCoroutine(tabCoroutine);
        }

        tabCoroutine = StartCoroutine(MoveToTab(index));
    }

    private IEnumerator MoveToTab(int index) {
        interactionBlocker.raycastTarget = true;
        if (tabs[index].scrollRect != null) {
            tabs[index].scrollRect.normalizedPosition = Vector2.one;
        }

        float duration = 0.2f;
        float elapsed = 0f;

        Vector2 start = container.anchoredPosition;
        Vector2 end = new(-containerWidth * index, container.anchoredPosition.y);

        while (elapsed < duration) {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            container.anchoredPosition = Vector2.Lerp(start, end, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        container.anchoredPosition = end;
        interactionBlocker.raycastTarget = false;

        tabCoroutine = null;
    }

    private void OnValidate() {
        //Tabs
        if (tabs.Count > 0) {
            for (int i = 0; i < tabs.Count; i++) {
                UITab uiTab = tabs[i];
                
                if (uiTab.rectTransform != null && uiTab.scrollRect == null) {
                    uiTab.scrollRect = uiTab.rectTransform.GetComponent<ScrollRect>();
                }
                else if (uiTab.scrollRect != null && uiTab.rectTransform == null) {
                    uiTab.rectTransform = uiTab.scrollRect.GetComponent<RectTransform>();
                }

                if (uiTab.rectTransform != null) {
                    uiTab.rectTransform.anchoredPosition = container.rect.size.x * i * Vector2.right;
                }
                
                tabs[i] = uiTab;
            }
        }
    }
}