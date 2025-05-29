using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelect : MonoBehaviour
{
    public GameObject instructionText;
    // Sub-menus and selection objects
    private GameObject modeSelector,
                        EditSubMenu,
                        AnalysisSubMenu,
                        SketchSubMenu;
    private Button translateButton,
                    rotateButton,
                    editNodes,
                    scaleButton,
                    measureButton,
                    extrudeButton,
                    extrudeToggleButton,
                    revolveButton,
                    spawnButton;
    public InputHandler playerInputHandler;


    private List<Button> toggles;

    public enum state
    {
        Scan,
        Edit,
        Analysis,
        Sketch
    }

    public state mode;
    public enum ToolSelectState
    {
        InactiveState,
        TranslateState,
        RotateState,
        EditState,
        ScaleState,
        measureState,
        ExtrudeState,
        extrudeToggleState,
        RevolveState,
        SpawnState
    }
    public ToolSelectState toolSelected;

    void Start()
    {
        mode = state.Scan;
        // Get UI buttons
        translateButton = GameObject.Find("TranslateTool").GetComponent<Button>();
        rotateButton = GameObject.Find("RotateTool").GetComponent<Button>();
        editNodes = GameObject.Find("PointEdit").GetComponent<Button>();
        scaleButton = GameObject.Find("Scaling").GetComponent<Button>();
        measureButton = GameObject.Find("Measure").GetComponent<Button>();
        extrudeButton = GameObject.Find("Extrude&Release").GetComponent<Button>();
        extrudeToggleButton = GameObject.Find("ExtrudeToggle").GetComponent<Button>();
        revolveButton = GameObject.Find("Revolve").GetComponent<Button>();
        spawnButton = GameObject.Find("SpawnButton").GetComponent<Button>();
        playerInputHandler = GameObject.Find("UserInputHandler").GetComponent<InputHandler>();

        // Get Sub Menus
        modeSelector = GameObject.Find("Mode");
        EditSubMenu = GameObject.Find("EditMenu");
        AnalysisSubMenu = GameObject.Find("Analysis");
        SketchSubMenu = GameObject.Find("SketchMenu");
        

        toggles = new List<Button>();
        toggles.Add(translateButton);
        toggles.Add(rotateButton);
        toggles.Add(editNodes);
        toggles.Add(scaleButton);
        toggles.Add(measureButton);
        toggles.Add(extrudeButton);
        toggles.Add(extrudeToggleButton);
        toggles.Add(revolveButton);
        toggles.Add(spawnButton);
        modeSelector.SetActive(false);


        // Clear tools on startup
        clearButtonActive();
        deactiveButtons();
    }

    float time;
    Boolean scanPeriodOver;
    void FixedUpdate()
    {
        if(scanPeriodOver) { return; }
        if(time < 10) {
            time += Time.deltaTime;
        }
        else {
            scanPeriodOver = true;
            instructionText.SetActive(false);
            activateButtons();
        }
    }

    public void selectTranslate() {
        clearButtonActive();
        translateButton.image.color = Color.gray;
        toolSelected = ToolSelectState.TranslateState;
    }
    public void selectRotate() {
        clearButtonActive();
        rotateButton.image.color = Color.gray;
        toolSelected = ToolSelectState.RotateState;

    }    
    public void pointEdit() {
        clearButtonActive();
        editNodes.image.color = Color.gray;
        toolSelected = ToolSelectState.EditState;
    }
    public void scaleEdit() {
        clearButtonActive();
        scaleButton.image.color = Color.gray;
        toolSelected = ToolSelectState.ScaleState;

    }
    public void measureeEdit() {
        clearButtonActive();
        measureButton.image.color = Color.gray;
        toolSelected = ToolSelectState.measureState;
    }
    public void extrudeEdit() {
        if (toolSelected == ToolSelectState.ExtrudeState)
        {
            clearButtonActive();
            extrudeButton.image.color = Color.white;
            toolSelected = ToolSelectState.InactiveState;
        }
        else
        {
            clearButtonActive();
            extrudeButton.image.color = Color.gray;
            toolSelected = ToolSelectState.ExtrudeState;
        }
    }
    public void extrudeToggleEdit() {
        if (toolSelected == ToolSelectState.extrudeToggleState)
        {
            clearButtonActive();
            extrudeToggleButton.image.color = Color.white;
            toolSelected = ToolSelectState.InactiveState;
        }
        else
        {
            clearButtonActive();
            extrudeToggleButton.image.color = Color.gray;
            toolSelected = ToolSelectState.extrudeToggleState;
        }
    }
    public void spawnEdit()
    {
        clearButtonActive();
        spawnButton.image.color = Color.gray;
        toolSelected = ToolSelectState.SpawnState;
    }
    public void revolveEdit()
    {
        clearButtonActive();
        if (playerInputHandler.objectHeld.TryGetComponent<RevolveTool>(out RevolveTool revolve))
        {
            revolve.useRevolveTool();
        }
        toolSelected = ToolSelectState.RevolveState;
    }
    private void clearButtonActive()
    {
        foreach (Button currButton in toggles)
        {
            currButton.image.color = Color.white;
        }
    }

    public void deactiveButtons() {
        foreach(Button currButton in toggles ) {
            currButton.gameObject.SetActive(false);
        }
    }

    public void activateButtons() {
        changeModeState();
        foreach(Button currButton in toggles ) {
            currButton.gameObject.SetActive(true);
        }
        modeSelector.SetActive(true);
        mode = state.Edit;
        instructionText.SetActive(false);
    }

    public void changeModeState()
    {
        TMP_Dropdown dropdown = modeSelector.GetComponent<TMP_Dropdown>();
        // To prevent potential misclicks we have a temporary no functionality
        // state that we go to when we change tools
        toolSelected = ToolSelectState.InactiveState;
        if (dropdown.value == 0)
        {
            AnalysisSubMenu.SetActive(false);
            EditSubMenu.SetActive(true);
            SketchSubMenu.SetActive(false);
            mode = state.Edit;
        }
        else if (dropdown.value == 1)
        {
            EditSubMenu.SetActive(false);
            AnalysisSubMenu.SetActive(true);
            SketchSubMenu.SetActive(false);
            mode = state.Analysis;
        }
        else if (dropdown.value == 2)
        {
            EditSubMenu.SetActive(false);
            AnalysisSubMenu.SetActive(false);
            SketchSubMenu.SetActive(true);
            mode = state.Sketch;
        }
    }
}
