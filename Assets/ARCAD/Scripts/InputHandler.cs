using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public bool debugMode = false;
    public Camera playerCam;
    //Debugging
    public TextMeshProUGUI rotationText, objectViewText, userPressed, pressPos,
                            userDoubleTap;

    public GameObject debuggingSphere;
    //inputs
    public InputAction leftClick;
    public InputAction pointerPosition;
    public InputAction touchOne;
    public InputAction touchTwo;
    private float currRotation = 0;
    private float rotSensitivity= 20f;


    public GameObject objectHolder;
    public GameObject cubePrefab;
    void Start()
    {
        // was to check whether it erros on invalid bindings which it does not
        leftClick.AddBinding("THIS BINDING IS INVALID");
        //enable the inputs that we use
        //Must enable otherwise input action will not work
        leftClick.Enable();
        touchOne.Enable();
        touchTwo.Enable();
        pointerPosition.Enable();
        if(!debugMode) {
            debuggingSphere.SetActive(false);
        }
        

    }

    private GameObject objectHeld;
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
            if(debugMode) {
                debuggingSphere.transform.position = hit.point;
            }

        }
        else {
            objectViewText.text = "nothing";
        }
        bool fliFlopedInput = flipFlop(leftClick.IsPressed());
        // If both fingers pressed down rotate
        if(leftClick.IsPressed()) {
            RaycastHit obectHit;
            Debug.Log("creating ray");

            Ray ray = playerCam.ScreenPointToRay(pointerPosition.ReadValue<Vector2>());
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 2f);

            if (Physics.Raycast(ray.origin, ray.direction * 100f, out obectHit)) {
                if(obectHit.transform.gameObject.tag == "SpawnObjects") {
                    objectHeld = obectHit.transform.gameObject;
                    // for pc just use e and r to double click on object and the cursor relative to center is the vector we check angle from
                    if(touchTwo.IsPressed()) {
                        Vector2 firstPoint;
                        Vector2 secondPoint;
                        // Both fingers are pressing the screen
                        if(Application.isMobilePlatform) {
                            firstPoint = touchOne.ReadValue<Vector2>();
                            secondPoint = touchTwo.ReadValue<Vector2>();

                        }
                        else {
                            firstPoint = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
                            secondPoint = pointerPosition.ReadValue<Vector2>();
                            Debug.Log("Double click works");
                        }   

                        if(!prevTwo) {
                            prevTwo = true;
                            orignallRot = firstPoint - secondPoint;
                            currRotation = 0f;
                            orignallObjectRot = obectHit.transform.eulerAngles;
                        }
                        else {
                            currRotation = Vector2.SignedAngle(orignallRot, secondPoint);
                        }
                        Debug.Log(currRotation);
                        userDoubleTap.text = "" + currRotation;

                        //get surface noraml so that we can rotate arond that axis
                                                Debug.Log("object interaction");
                        obectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = false;
                        //recast ray to get position behind object
                        RaycastHit surface;
                        Physics.Raycast(ray.origin, ray.direction * 100f, out surface);
                        objectHeld.transform.localEulerAngles = new Vector3(orignallObjectRot.x,
                                                                            orignallObjectRot.y + currRotation,
                                                                            orignallObjectRot.z);
                        obectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = true;;

                    }
                    else {
                        userDoubleTap.text = "One finger down and we are translating object";
                        // One finger is pressing the screen
                                                Debug.Log("object interaction");
                        obectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = false;
                        //recast ray to get position behind object
                        RaycastHit surface;
                        Physics.Raycast(ray.origin, ray.direction * 100f, out surface);
                        obectHit.transform.position = surface.point + (surface.normal*
                                                            (obectHit.transform.gameObject.GetComponent<MeshRenderer>().bounds.size.x*2) * 
                                                            obectHit.transform.localScale.x);
                        obectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = true;;
                    }
                }
                else if(fliFlopedInput){
                    Debug.Log(obectHit.point); 
                    GameObject spawnedCube = Instantiate(cubePrefab, objectHolder.transform);
                    spawnedCube.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    // For some reason you need to multiply by 2
                    spawnedCube.transform.position = obectHit.point + (obectHit.normal*
                                                        (spawnedCube.GetComponent<MeshRenderer>().bounds.size.x*2) * 
                                                        spawnedCube.transform.localScale.x);

                }
            }

        }
        // Reset double touchscreen mechanic
        if(!touchOne.IsPressed() || !touchTwo.IsPressed()) {
            prevTwo = false;
            orignallRot = new Vector3(0,0,0);
            orignallObjectRot = new Vector3(0,0,0);
            Debug.Log("reset");
        }
        else {
            rotateObject();
        }


    }

    public void rotateObject() {
        Vector2 firstPoint;
        Vector2 secondPoint;
        // Both fingers are pressing the screen
        if(Application.isMobilePlatform) {
            firstPoint = touchOne.ReadValue<Vector2>();
            secondPoint = touchTwo.ReadValue<Vector2>();

        }
        else {
            firstPoint = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
            secondPoint = pointerPosition.ReadValue<Vector2>();
            Debug.Log("Double click works");
        }   

        if(!prevTwo) {
            prevTwo = true;
            orignallRot = firstPoint - secondPoint;
            currRotation = 0f;
        }
        else {
            currRotation = Vector2.SignedAngle(orignallRot, secondPoint);
        }
        if(Application.isMobilePlatform) {
            objectHeld.transform.localEulerAngles = new Vector3(orignallObjectRot.x,
                                                    orignallObjectRot.y - (currRotation * rotSensitivity),
                                                    orignallObjectRot.z);
        }
        else {
            objectHeld.transform.localEulerAngles = new Vector3(orignallObjectRot.x,
                                                    orignallObjectRot.y + currRotation,
                                                    orignallObjectRot.z);
        } 
    }
    private Vector2 orignallRot;
    private Vector3 orignallObjectRot;
    bool prevTwo = false;
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
