using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public bool debugMode = false;
    public Camera playerCam;
    //Debugging
    public TextMeshProUGUI rotationText, objectViewText, userPressed, pressPos,
                            userDoubleTap, secondButtonPress, itemHeldID;
    public GameObject debuggingSphere;

    // User inputs
    public InputAction leftClick;
    public InputAction touchTwoPressed;
    public InputAction pointerPosition;
    public InputAction touchOne;
    public InputAction touchTwo;

    // Adjustables (related to user input)
    private float currRotation = 0;
    private float rotSensitivity= 20f;
    public TMP_Dropdown objectSelector;

    // Object handling
    public GameObject objectHolder;
    public GameObject cubePrefab;
    // Measuring
    public GameObject measureMarker;
    public GameObject measureText;
    private Object[] spawnableObjects;
    private ToolSelect playerToolSelect;
    public bool fliFlopedInput;
    float screenDiag;
    void Start()
    {
        screenDiag = math.sqrt(math.pow(Screen.width, 2) + math.pow(Screen.height, 2));
        // Get Game Objects/ scripts
        playerToolSelect = GetComponent<ToolSelect>();
        //enable the inputs that we use
        //Must enable otherwise input action will not work
        leftClick.Enable();
        touchOne.Enable();
        touchTwo.Enable();
        pointerPosition.Enable();
        touchTwoPressed.Enable();
        if(!debugMode) {
            debuggingSphere.SetActive(false);
            rotationText.gameObject.SetActive(false);
            pressPos.gameObject.SetActive(false);
            objectViewText.gameObject.SetActive(false);
            userPressed.gameObject.SetActive(false);
            secondButtonPress.gameObject.SetActive(false);
            userDoubleTap.gameObject.SetActive(false);
            itemHeldID.gameObject.SetActive(false);
        }
        // Loads all files in the path Resources/Prefabs.
        // These files MUST be in the resources folder.
        // If you want to add more files to the instantiatable prefabs
        // list simply drag and drop your prefab into the folder labeled
        // prefabs
        spawnableObjects = Resources.LoadAll("Prefabs/", typeof(GameObject));
    }

    private GameObject objectHeld;
    // Update is called once per frame
    void FixedUpdate()
    {
        if(playerToolSelect.mode == ToolSelect.state.Scan) { return; }
        //Assign Debugging text
        if (debugMode) {
            rotationText.text = "" + playerCam.transform.rotation;
            pressPos.text = "position: " + pointerPosition.ReadValue<Vector2>();
            userPressed.text = "user clicked" +  leftClick.IsPressed();
            secondButtonPress.text = "second" + touchTwoPressed.IsPressed();
        }



        bool secondPlayerClick = touchTwoPressed.IsPressed();
        fliFlopedInput = flipFlop(leftClick.IsPressed());



        // If both fingers pressed down rotate
        if(leftClick.IsPressed()) {
            RaycastHit objectHit;
            Ray ray = playerCam.ScreenPointToRay(pointerPosition.ReadValue<Vector2>());

            if (Physics.Raycast(ray.origin, ray.direction * 100f, out objectHit)) {
                GameObject currObjecthit = objectHit.transform.gameObject;
                if(currObjecthit.tag.Equals("SpawnObjects")) {
                    objectHeld = currObjecthit;
                }

                if(fliFlopedInput && playerToolSelect.measureActive) {
                    measureToolSpawn(objectHit);
                }
                else if(fliFlopedInput && currObjecthit.tag.Equals("Interactable") && playerToolSelect.EditActive) {
                    currObjecthit.GetComponent<RevolveTool>().markPoint(objectHit.point);
                }
                else if(currObjecthit.tag.Equals("SpawnObjects") && playerToolSelect.TranslateActive) {
                    objectHeld = currObjecthit;
                    // for pc just use e and r to double click on object and the cursor relative to center is the vector we check angle from
                    translateObject(objectHit, ray);
                }
                else if(!EventSystem.current.IsPointerOverGameObject() && 
                            !currObjecthit.tag.Equals("SpawnObjects") && !currObjecthit.tag.Equals("Interactable")
                             && fliFlopedInput){
                    // Spawn object
                    objectHeld = spawnObject(objectHit);
                }

            }

        }
        // Reset double touchscreen mechanic
        if(!touchOne.IsPressed() || !touchTwo.IsPressed()) {
            prevTwo = false;
            orignallRot = new Vector3(0,0,0);
            orignallObjectRot = new Vector3(0,0,0);
        }
        else if(touchOne.IsPressed() && !touchTwo.IsPressed()) {

        }
        else if(!EventSystem.current.IsPointerOverGameObject()  && touchOne.IsPressed() && touchTwo.IsPressed()) {
            if(playerToolSelect.RotateActive){
                // rotate obejct
                rotateObject();
            }
            else if(playerToolSelect.ScaleActive) {
                scaleObject();
            }
        }
    }

    GameObject pointOne;
    GameObject pointTwo;
    GameObject measureTextCurr;
    // Instantiates new point to measure to and displays measurement in worldspace
    private void measureToolSpawn(RaycastHit objectHit) {
        Debug.Log("spawn");
        GameObject currMarker = Instantiate(measureMarker);
        currMarker.transform.position = objectHit.point;
        if(pointOne == null) {
            pointOne = currMarker;
        }
        else if(pointTwo == null) {
            pointTwo = currMarker;
        }
        else {
            Destroy(pointOne);
            pointOne = pointTwo;
            pointTwo = currMarker;
        }
        float Distance = Vector3.Distance(pointOne.transform.position, pointTwo.transform.position);
        if(measureTextCurr == null) {
            measureTextCurr = Instantiate(measureText);
        }
        measureTextCurr.gameObject.transform.position = ((pointOne.transform.position - pointTwo.transform.position) / 2) + pointTwo.transform.position;
        measureTextCurr.gameObject.transform.position += objectHit.normal * 0.2f;
        // Convert from unity world to CM
        // I measured 30 CM with a ruler and got 0.293029
        Distance = (Distance / 0.293029f) * 30f;
        measureTextCurr.transform.GetChild(0).GetComponent<TMP_Text>().text = "" + Distance + " CM";
        measureTextCurr.transform.LookAt(playerCam.transform.position);
    }

    public void rotateObject() {
        if(debugMode) {
            userDoubleTap.text = "" + currRotation;
        }
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

    public GameObject spawnObject(RaycastHit objectHit) {
        Debug.Log("tried to spawn " + objectSelector.value);
        // For some reason you need to multiply by 2
        GameObject gameObjectSpawning;
        if(objectSelector.value >= spawnableObjects.Length) {
            Debug.Log("Unable to spawn selected object!");
            
            return null;
        }
        gameObjectSpawning = (GameObject)spawnableObjects[objectSelector.value];
        GameObject spawnedCube = Instantiate(gameObjectSpawning, objectHolder.transform);
        spawnedCube.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        spawnedCube.transform.position = objectHit.point + (objectHit.normal*
                                            (spawnedCube.GetComponent<MeshRenderer>().bounds.size.x*2) * 
                                            spawnedCube.transform.localScale.x);
        return spawnedCube;
    }

    public void translateObject(RaycastHit objectHit, Ray ray) {
        if(debugMode) {
            userDoubleTap.text = "One finger down and we are translating object";
        }
        // One finger is pressing the screen
        objectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = false;
        //recast ray to get position behind object
        RaycastHit surface;
        Physics.Raycast(ray.origin, ray.direction * 100f, out surface);
        objectHit.transform.position = surface.point + (surface.normal  *
                                            objectHit.transform.gameObject.GetComponent<MeshRenderer>().bounds.size.x * 
                                            objectHit.transform.localScale.x);
        objectHit.transform.gameObject.GetComponent<BoxCollider>().enabled = true;

        // Check if the obejct that we transformed is moving to a wall and orient it in the rotation of the wall
        if( !Mathf.Approximately(Vector3.Dot(surface.normal, Vector3.up), 1f)) {
            Debug.Log("Algining" + surface.normal );
            Debug.Log("Algining" + Vector3.Dot(surface.normal, new Vector3(0f,1f,0f) ));
            objectHeld.transform.eulerAngles = new Vector3(orignallObjectRot.x, 
                                                            orignallObjectRot.y + Vector3.Angle(surface.normal, new Vector3(-1,0,0)), 
                                                            orignallObjectRot.z);
        }
    }
    // Scales object by the distance between the two fingers on mobile
    // and the distance from the cursor to the center on PC
    private void scaleObject() {
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
        if(!touchOne.IsPressed() || !touchTwo.IsPressed()) { return; }
        float dist = Vector2.Distance(firstPoint, secondPoint) / Screen.currentResolution.height;
        objectHeld.transform.localScale = new Vector3(dist, 
                                                        dist, 
                                                        dist);
        Debug.Log("we are going here");
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
