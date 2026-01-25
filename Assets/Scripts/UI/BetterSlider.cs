using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetterSlider : Slider
{
    [Header("Text Value Settings")]
    [SerializeField] private TMP_InputField text;
    [SerializeField] private string format;
    
    [Header("Snap Settings")]
    [SerializeField] private float snapInterval;

    [Header("Buttons")] 
    [SerializeField] private float buttonIncrement = 1f;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    
    protected override void Start()
    {
        base.Start();
        text.text = m_Value.ToString(format);
        text.onEndEdit.AddListener(ParseTextInput);
        
        leftButton.onClick.AddListener(() => ButtonClicked(false));
        rightButton.onClick.AddListener(() => ButtonClicked(true));
    }

    private void ParseTextInput(string input)
    {
        if (float.TryParse(input, out float parsedValue))
        {
            Set(parsedValue, false);
        }
        else
        {
            text.text = m_Value.ToString(format);
        }
    }

    private void ButtonClicked(bool positive)
    {
        float increment = positive ? buttonIncrement : -buttonIncrement;
        float newValue = m_Value + increment;

        Set(newValue, false);
    }

    protected override void Set(float input, bool sendCallback = true)
    {
        base.Set(input, sendCallback);
        
        if (snapInterval != 0f)
        {
            input = SnapSlider();
        }
        
        base.Set(input, sendCallback);
        
        text.text = m_Value.ToString(format);
    }
    
    private float SnapSlider()
    {
        return Mathf.Round(m_Value / snapInterval) * snapInterval;
    }
}