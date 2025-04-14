using TMPro;
using Unity.Tutorials.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class InputHandler : MonoBehaviour
{
    public bool debugMode = false;
    public Camera playerCam;
    //Debugging
    public TextMeshProUGUI rotationText, objectViewText, userPressed, pressPos;

    public GameObject debuggingSphere;
    //inputs
    public InputAction leftClick;
    public InputAction pointerPosition;
    


    public GameObject objectHolder;
    public GameObject cubePrefab;
    void Start()
    {
        // was to check whether it erros on invalid bindings which it does not
        leftClick.AddBinding("THIS BINDING IS INVALID");
        //enable the inputs that we use
        //Must enable otherwise input action will not work
        leftClick.Enable();
        pointerPosition.Enable();

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //Assign Debugging text
        rotationText.text = "" + playerCam.transform.rotation;
        pressPos.text = "position: " + pointerPosition.ReadValue<Vector2>();
        userPressed.text = "user clicked" +  leftClick.IsPressed();

        //What is the user looking at
        RaycastHit hit;
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        { 
            //Debug.DrawRay(playerCam.transform.position, playerCam.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow); 
            objectViewText.text = hit.transform.name;
            debuggingSphere.transform.position = hit.point;
        }
        else {
            objectViewText.text = "nothing";
        }

        //What did the user press on
        if(flipFlop(leftClick.IsPressed())) {
            RaycastHit obectHit;
            Debug.Log("creating ray");
            // The direction that the camera is facing (transform.forwards) will always point towards the center
            // of the camera thus making it obselete for our purposes
            // Vector3 cursorWorldPos = playerCam.ScreenToWorldPoint(new Vector3(pointerPosition.ReadValue<Vector2>().x, 
            //         pointerPosition.ReadValue<Vector2>().y, playerCam.nearClipPlane));

            Ray ray = playerCam.ScreenPointToRay(pointerPosition.ReadValue<Vector2>());
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);

            if (Physics.Raycast(ray.origin,
                    ray.direction * 100f, out obectHit)) {
                Debug.Log(obectHit.point); 
                GameObject spawnedCube = Instantiate(cubePrefab, objectHolder.transform);
                spawnedCube.transform.position = obectHit.point;
                spawnedCube.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }

        }


    }
    bool prev = false;
    private bool flipFlop(bool input) {
        bool returnVal = false;
        if(input && !prev) {
            returnVal = true;
        }
        prev = input;
        return returnVal;
    }
}
