using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelect : MonoBehaviour
{

    private Button translateButton;
    private Button rotateButton;
    private Button editNodes;
    private Button scaleButton;
    private Button measureButton;
    private List<Button> toggles;


    public Boolean TranslateActive;
    public Boolean RotateActive;
    public Boolean EditActive;
    public Boolean ScaleActive;
    public Boolean measureActive;
    void Start()
    {
        // Get UI buttons
        translateButton = GameObject.Find("TranslateTool").GetComponent<Button>();
        rotateButton = GameObject.Find("RotateTool").GetComponent<Button>();
        editNodes = GameObject.Find("RotGenerateEdit").GetComponent<Button>();
        scaleButton = GameObject.Find("Scaling").GetComponent<Button>();
        measureButton = GameObject.Find("Measure").GetComponent<Button>();

        toggles = new List<Button>();
        toggles.Add(translateButton);
        toggles.Add(rotateButton);
        toggles.Add(editNodes);
        toggles.Add(scaleButton);
        toggles.Add(measureButton);

        // Clear tools on startup
        clearButtonActive();
    }
    public void selectTranslate() {
        clearButtonActive();
        translateButton.image.color = Color.gray;
        TranslateActive = true;
    }
    public void selectRotate() {
        clearButtonActive();
        rotateButton.image.color = Color.gray;
        RotateActive = true;
    }    
    public void pointEdit() {
        clearButtonActive();
        editNodes.image.color = Color.gray;
        EditActive = true;
    }
    public void scaleEdit() {
        clearButtonActive();
        scaleButton.image.color = Color.gray;
        ScaleActive = true;
    }
    public void measureeEdit() {
        clearButtonActive();
        measureButton.image.color = Color.gray;
        measureActive = true;
    }
    private void clearButtonActive() {
        foreach(Button currButton in toggles ) {
            currButton.image.color = Color.white;
        }
        TranslateActive = false;
        RotateActive = false;
        EditActive = false;
        ScaleActive = false;
        measureActive = false;
    }
}
