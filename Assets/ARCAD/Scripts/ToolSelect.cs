using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelect : MonoBehaviour
{

    private Button translateButton;
    private Button rotateButton;
    private Button editNodes;
    private List<Button> toggles;
    void Start()
    {
        // Get UI buttons
        // translateButton = GameObject.Find("TranslateTool").GetComponent<Button>();
        // rotateButton = GameObject.Find("RotateTool").GetComponent<Button>();
        // editNodes = GameObject.Find("RotGenerateEdit").GetComponent<Button>();

        // toggles = new List<Button>();
        // toggles.Add(translateButton);
        // toggles.Add(rotateButton);
        // toggles.Add(editNodes);
    }
    public void selectTranslate() {
        foreach(Button currButton in toggles ) {
            Debug.Log(currButton);
            translateButton.image.color = Color.white;
        }
        translateButton.image.color = new Color(200, 200, 200);
    }
    public void selectTRotate() {
        
    }    
    public void pointEdit() {
        
    }
}
