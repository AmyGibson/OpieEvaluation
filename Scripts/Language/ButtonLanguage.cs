using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLanguage : MonoBehaviour {

    public Button buttonComponent;
    public Text languageLabel;
    public Text SelectionDisplay;

    private string language;

    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.AddListener(HandleClick);
    }

    public void Setup(string lang)
    {
        language = lang;
        languageLabel.text = lang;


    }

    public void HandleClick()
    {
        SelectionDisplay.text = language;
    }
}
