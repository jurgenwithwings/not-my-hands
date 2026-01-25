using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEnum : MonoBehaviour {
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private RectTransform textContainer;
    [SerializeField] private RectTransform widgetsContainer;

    [SerializeField] private Color activeWidgetColor = new Color(209f / 255f, 54f / 255f, 54f / 255f);
    [SerializeField] private Color inactiveWidgetColor = Color.white;

    private TMP_Text optionText;
    private Image optionWidget;

    private float textWidth => textContainer.rect.size.x;

    [SerializeField] private List<string> options = new();
    private List<TMP_Text> optionTexts = new();
    private List<Image> optionWidgets = new();

    private Coroutine menuAnimationCoroutine;

    public int value { get; private set; }

    public UnityEvent<int> OnIndexChanged;
    
    private bool initialised = false;

    private void Awake() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(textContainer);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());

        leftButton.onClick.AddListener(() => ButtonClicked(false));
        rightButton.onClick.AddListener(() => ButtonClicked(true));

        optionText = textContainer.GetChild(0).GetComponent<TMP_Text>();
        optionText.text = options[0];
        optionTexts.Add(optionText);

        optionWidget = widgetsContainer.GetChild(0).GetComponent<Image>();
        optionWidgets.Add(optionWidget);

        CreateOptions();

        Set(value);

        StartCoroutine(CorrectOptionSize());
    }

    private void CreateOptions() {
        for (int i = 1; i < options.Count; i++) {
            optionTexts.Add(Instantiate(optionText, textContainer).GetComponent<TMP_Text>());
            optionTexts[i].text = options[i];


            optionWidgets.Add(Instantiate(optionWidget, widgetsContainer).GetComponent<Image>());
        }

        foreach (Image widget in optionWidgets) {
            widget.color = inactiveWidgetColor;
        }
        
        initialised = true;
    }

    private void ClearOptions() {
        for (int i = optionTexts.Count - 1; i >= 0; i--) {
            Destroy(optionTexts[i].gameObject);
            optionTexts.Remove(optionTexts[i]);
            Destroy(optionWidgets[i].gameObject);
            optionWidgets.Remove(optionWidgets[i]);
        }
    }

    private IEnumerator CorrectOptionSize() {
        //Wait 2 frames cos unity is silly
        yield return null;
        yield return null;

        for (int i = 1; i < options.Count; i++) {
            optionTexts[i].rectTransform.anchoredPosition = textWidth * i * Vector2.right;
        }

        Set(value);
    }

    public void Set(int index) {
        if (index < 0 || index >= options.Count) {
            index %= options.Count;
        }
        
        if (!initialised) {
            //Debug.Log("Not initialised yet", this);
            value = index;
            return;
        }
        optionWidgets[value].color = inactiveWidgetColor;
        
        optionWidgets[index].color = activeWidgetColor;

        value = index;

        if (menuAnimationCoroutine != null) {
            StopCoroutine(menuAnimationCoroutine);
        }

        menuAnimationCoroutine = StartCoroutine(Animate(value));

        OnIndexChanged?.Invoke(index);
    }


    private void ButtonClicked(bool positive) {
        int newIndex = value;
        if (positive && value < options.Count - 1) {
            newIndex++;
        }
        else if (!positive && value > 0) {
            newIndex--;
        }
        else {
            return;
        }

        Set(newIndex);
    }

    private IEnumerator Animate(int index) {
        float duration = 0.4f;
        float elapsed = 0f;

        Vector2 start = textContainer.anchoredPosition;
        Vector2 end = new Vector2(-textWidth * index, textContainer.anchoredPosition.y);

        while (elapsed < duration) {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            textContainer.anchoredPosition = Vector2.Lerp(start, end, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        textContainer.anchoredPosition = end;

        menuAnimationCoroutine = null;
    }

    /*Should add options in editor but doesn't work properly
#if UNITY_EDITOR
    private void OnValidate() {
        
        //Different Amount of options
        if (optionTexts.Count != options.Count) {
            CreateOptions();
        }
        else {
            bool isDifferent = false;
            for (int i = 0; i < options.Count; i++) {
                if (optionTexts[i].text != options[i]) {
                    isDifferent = true;
                }

                if (isDifferent) {
                    CreateOptions();
                    break;
                }
            }
        }
    }
#endif*/
}