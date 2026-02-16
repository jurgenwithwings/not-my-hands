using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UITab {
    [Header("Required")]
    public Button button;
    public RectTransform rectTransform;
    [Space]
    [Header("Optional")]
    public ScrollRect scrollRect;

    public UITab(Button button, RectTransform rectTransform) {
        this.rectTransform = rectTransform;
        this.button = button;

        scrollRect = null;
    }
}

public class UITabController : MonoBehaviour {
    [Header("Tabs")] 
    [SerializeField] private RectTransform container;

    [SerializeField] private float tabSpacing = 20f;
    [SerializeField] private Image interactionBlocker;
    [SerializeField] private List<UITab> tabs;
    private float containerWidth => container.rect.size.x + tabSpacing;
    private Coroutine tabCoroutine;
    
    private bool init = false;

    private void Awake() {
        for (int i = 0; i < tabs.Count; i++) {
            int index = i;
            tabs[i].button.onClick.AddListener(() => OnTabButtonClicked(index));
        }
    }

    private void OnEnable() {
        if (!init) {
            SetTabPositions();
        }
    }
    
    //Waits to set init to true so that the canvas has 3 frames to not be ass.
    private IEnumerator Init() {        
        yield return null;
        yield return null;
        yield return null;

        init = true;
    }

    private void SetTabPositions() {
        for (int i = 0; i < tabs.Count; i++) {
            Vector2 currPos = tabs[i].rectTransform.anchoredPosition;
            tabs[i].rectTransform.anchoredPosition = containerWidth * i * Vector2.right;
            print($"Curr Pos: {currPos} | ContainerSize: {container.rect.x} + Spacing: {tabSpacing} * i: {i} = {(container.rect.x + tabSpacing) * i})");
        }
        StartCoroutine(Init());
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
}