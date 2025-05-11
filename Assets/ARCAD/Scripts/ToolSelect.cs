using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelect : MonoBehaviour
{

    private Button translateButton;
    private Button rotateButton;
    private Button editNodes;
    private List<Button> toggles;


    public Boolean TranslateActive;
    public Boolean RotateActive;
    public Boolean EditActive;
    void Start()
    {
        // Get UI buttons
        translateButton = GameObject.Find("TranslateTool").GetComponent<Button>();
        rotateButton = GameObject.Find("RotateTool").GetComponent<Button>();
        editNodes = GameObject.Find("RotGenerateEdit").GetComponent<Button>();

        toggles = new List<Button>();
        toggles.Add(translateButton);
        toggles.Add(rotateButton);
        toggles.Add(editNodes);
    }
    public void selectTranslate() {
        clearButtonActive();
        foreach(Button currButton in toggles ) {
            Debug.Log(currButton);
            currButton.image.color = Color.white;
        }
        translateButton.image.color = Color.gray;
        TranslateActive = true;
    }
    public void selectRotate() {
        clearButtonActive();
        foreach(Button currButton in toggles ) {
            Debug.Log(currButton);
            currButton.image.color = Color.white;
        }
        rotateButton.image.color = Color.gray;
        RotateActive = true;
    }    
    public void pointEdit() {
        clearButtonActive();
        foreach(Button currButton in toggles ) {
            Debug.Log(currButton);
            currButton.image.color = Color.white;
        }
        editNodes.image.color = Color.gray;
        EditActive = true;
    }
    private void clearButtonActive() {
        TranslateActive = false;
        RotateActive = false;
        EditActive = false;
    }
}
