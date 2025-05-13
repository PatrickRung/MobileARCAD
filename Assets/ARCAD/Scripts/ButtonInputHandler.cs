using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ButtonInputHandler : MonoBehaviour
{
    public ARCameraManager ARCamManager;
    public ToolSelect playerToolselect;

    // Also a shortcut to enable tools quickyl for debuggin
    public void toggleCameraFeed() {
        if(ARCamManager.enabled) {
            ARCamManager.enabled = false;
        }
        else {
            ARCamManager.enabled = true;
        }
        playerToolselect.activateButtons();
    }
}
