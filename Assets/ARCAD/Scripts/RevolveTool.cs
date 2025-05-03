using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RevolveTool : MonoBehaviour
{
    // User interaction
    public InputAction userInteract;
    public InputAction userInteractLoc;
    private Camera playerCam;
    private bool primaryClickPressed;
    public GameObject curvaturePointMarker;
    // Mesh detail adjustables
    public int subdivisions = 8;
    public List<Vector2> curvePoints;
    // Mesh data
    private Vector2[] UVs;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        primaryClickPressed = false;
        userInteract.Enable();
    }

    // This script will need to reinitialize funcitonality of the drawing tool
    // when the tool was minimized
    void Awake()
    {
        playerCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        if(curvaturePointMarker == null) {
            curvaturePointMarker = GameObject.Find("CurvatureMarker");
        }
    }

    // Update is called once per frame
    public void markPoint(Vector3 position) {
        Debug.Log(position);
        Debug.Log(curvePoints.Count);
        GameObject currMarker = Instantiate(curvaturePointMarker, gameObject.transform);
        currMarker.transform.position = position;
        curvePoints.Add(new Vector2(position.x, position.y));
        if(curvePoints.Count > 4) {
            ComputeMeshData();
            generateMesh();
        }
    }

    private void ComputeMeshData()
    {
        // TODO: Compute and set vertex positions, normals, UVs, and triangle faces
        // You will want to use curvePoints and subdivisions variables, and you will
        // want to change the size of these arrays
        int totalVertices = (subdivisions + 1) * curvePoints.Count;
        vertices = new Vector3[totalVertices];
        normals = new Vector3[totalVertices];
        UVs = new Vector2[totalVertices];
        triangles = new int[subdivisions * (curvePoints.Count - 1) * 6];
        int currCount = 0;
        int currTriangle = 0;
        for(int i = 0; i < subdivisions + 1; i++) {
            float currAngle = ((2f * Mathf.PI) / subdivisions) * i;
            for(int j = 0; j < curvePoints.Count; j++) {
                vertices[currCount] = new Vector3(Mathf.Sin(currAngle) * curvePoints[j].x,
                curvePoints[j].y, Mathf.Cos(currAngle) * curvePoints[j].x);

                if(j < curvePoints.Count - 1 && i < subdivisions) {
                    // Make triangle 1 for surface
                    triangles[currTriangle] = currCount;
                    triangles[currTriangle + 1] = (currCount + 1) % totalVertices;
                    triangles[currTriangle + 2] = (currCount + curvePoints.Count) % totalVertices;
                    currTriangle += 3;
                    // Make triangle 2 for surface
                    triangles[currTriangle] = (currCount + curvePoints.Count) % totalVertices;
                    triangles[currTriangle + 1] = (currCount + 1) % totalVertices;
                    triangles[currTriangle + 2] = (currCount + curvePoints.Count + 1) % totalVertices;
                    currTriangle += 3;
                }
                currCount++;
            } 
        }

        // Iterate through to calculate normal
        float heightLength = 0;
        for(int i = 0; i < curvePoints.Count - 1; i++) {
            heightLength += Vector3.Distance(vertices[i], vertices[i + 1]);
        }
        currCount = 0;
        for(int i = 0; i < subdivisions + 1; i++) {
            float currAngle = ((2f * Mathf.PI) / subdivisions) * i;
            float currHeightLength = 0;
            for(int j = 0; j < curvePoints.Count; j++) {
                // Calculate normals for not last row
                if(i == 0) {
                    Vector3 currTangVec = new Vector3(1, 0, 0);
                    Vector3 diffVec;
                    if(j == 0) {
                        diffVec = vertices[currCount + 1] - vertices[currCount];
                    }
                    else if(j == curvePoints.Count - 1) {
                        diffVec = vertices[currCount] - vertices[currCount - 1];
                    }
                    else {
                        diffVec = vertices[currCount + 1] - vertices[currCount - 1];
                    }
                    normals[currCount] = Vector3.Cross(diffVec, currTangVec);
                    float mag = Mathf.Sqrt(Mathf.Pow(normals[currCount].x, 2) + Mathf.Pow(normals[currCount].y, 2) + Mathf.Pow(normals[currCount].z, 2));
                    normals[currCount] = new Vector3(normals[currCount].x / mag, normals[currCount].y / mag, normals[currCount].z / mag);

                }
                else {
                    normals[currCount] = new Vector3((normals[currCount % curvePoints.Count].x * Mathf.Cos(currAngle)) +
                                                (normals[currCount % curvePoints.Count].z * Mathf.Sin(currAngle)), 
                                                normals[currCount % curvePoints.Count].y,
                                                (-1 * normals[currCount % curvePoints.Count].x * Mathf.Sin(currAngle)) +
                                                (normals[currCount % curvePoints.Count].z * Mathf.Cos(currAngle)));
                }
                UVs[currCount % totalVertices] = new Vector2((float)i /  (float)subdivisions, currHeightLength / heightLength);
                currHeightLength += Vector3.Distance(vertices[j], vertices[j + 1]);
                currCount++;
            }
        }
    }
    public Mesh currMesh;
    private void generateMesh() {
        currMesh = new Mesh();
        GameObject newGameObjectMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newGameObjectMesh.GetComponent<MeshFilter>().mesh = currMesh;
    }
}