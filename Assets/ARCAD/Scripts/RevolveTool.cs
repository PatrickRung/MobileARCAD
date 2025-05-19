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
    public GameObject curvaturePointMarker;
    // Mesh detail adjustables
    public int subdivisions = 8;
    public List<Vector2> curvePoints;
    public List<Vector2> curvePointsRefined;
    // Mesh data
    private Vector2[] UVs;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;

    private Vector2 objectScreenPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectScreenPos = new Vector2(playerCam.WorldToScreenPoint(gameObject.transform.position).x,
                                        playerCam.WorldToScreenPoint(gameObject.transform.position).y);
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
        GameObject currMarker = Instantiate(curvaturePointMarker, gameObject.transform);
        currMarker.transform.position = position;
        curvePoints.Add(new Vector2(position.x - gameObject.transform.position.x, position.y - gameObject.transform.position.y));
    }

    public void useRevolveTool()
    {
        if(curvePoints.Count > 4) {
            RefineCurvePoints();
            ComputeMeshData();
            generateMesh();
        }
    }
    float extrudeLength;
    public void useExtrudeTool(Vector2 pointerPos)
    {
        extrudeLength = Vector2.Distance(pointerPos, objectScreenPos);
        Debug.Log(extrudeLength);
    }
    public void finishExtrude()
    {
        
    }
    public int u = 1;
    private void RefineCurvePoints() {
        // Use Catmull-Room spline equation to smoothen out point
        // formula from here https://www.cs.cmu.edu/~fp/courses/graphics/asst5/catmullRom.pdf
        curvePoints.Add(curvePoints[curvePoints.Count - 1]);
        curvePointsRefined.RemoveAll(item => true);
        float t = 0.5f;
        for(int i = 0; i < curvePoints.Count; i++) {
            curvePointsRefined.Add(curvePoints[i]);
            if(i >= 2 && i < curvePoints.Count - 2) {
                // catmul rom formula from CSE 457
                //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
                Vector3 a = 2f * curvePoints[i]; // i c1 from the formula is the ith element
                Vector3 b = curvePoints[i + 1] - curvePoints[i - 1];
                Vector3 c = 2f * curvePoints[i - 1] - 5f * curvePoints[i] + 4f * curvePoints[i + 1] - curvePoints[i + 2];
                Vector3 d = -curvePoints[i - 1] + 3f * curvePoints[i] - 3f * curvePoints[i + 1] + curvePoints[i + 2];

                //The cubic polynomial: a + b * t + c * t^2 + d * t^3
                Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
                curvePointsRefined.Add(pos);
            }
        }
        curvePoints.RemoveAt(curvePoints.Count - 1);

    }

    private void ComputeMeshData()
    {
        int totalVertices = (subdivisions + 1) * curvePointsRefined.Count;
        vertices = new Vector3[totalVertices];
        normals = new Vector3[totalVertices];
        UVs = new Vector2[totalVertices];
        triangles = new int[subdivisions * (curvePointsRefined.Count - 1) * 6];
        int currCount = 0;
        int currTriangle = 0;
        for(int i = 0; i < subdivisions + 1; i++) {
            float currAngle = ((2f * Mathf.PI) / subdivisions) * i;
            for(int j = 0; j < curvePointsRefined.Count; j++) {
                vertices[currCount] = new Vector3(Mathf.Sin(currAngle) * curvePointsRefined[j].x,
                curvePointsRefined[j].y, Mathf.Cos(currAngle) * curvePointsRefined[j].x);

                if(j < curvePointsRefined.Count - 1 && i < subdivisions) {
                    // Make triangle 1 for surface
                    triangles[currTriangle] = currCount;
                    triangles[currTriangle + 1] = (currCount + 1) % totalVertices;
                    triangles[currTriangle + 2] = (currCount + curvePointsRefined.Count) % totalVertices;
                    currTriangle += 3;
                    // Make triangle 2 for surface
                    triangles[currTriangle] = (currCount + curvePointsRefined.Count) % totalVertices;
                    triangles[currTriangle + 1] = (currCount + 1) % totalVertices;
                    triangles[currTriangle + 2] = (currCount + curvePointsRefined.Count + 1) % totalVertices;
                    currTriangle += 3;
                }
                currCount++;
            } 
        }

        // Iterate through to calculate normal
        float heightLength = 0;
        for(int i = 0; i < curvePointsRefined.Count - 1; i++) {
            heightLength += Vector3.Distance(vertices[i], vertices[i + 1]);
        }
        currCount = 0;
        for(int i = 0; i < subdivisions + 1; i++) {
            float currAngle = ((2f * Mathf.PI) / subdivisions) * i;
            float currHeightLength = 0;
            for(int j = 0; j < curvePointsRefined.Count; j++) {
                // Calculate normals for not last row
                if(i == 0) {
                    Vector3 currTangVec = new Vector3(1, 0, 0);
                    Vector3 diffVec;
                    if(j == 0) {
                        diffVec = vertices[currCount + 1] - vertices[currCount];
                    }
                    else if(j == curvePointsRefined.Count - 1) {
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
                    normals[currCount] = new Vector3((normals[currCount % curvePointsRefined.Count].x * Mathf.Cos(currAngle)) +
                                                (normals[currCount % curvePointsRefined.Count].z * Mathf.Sin(currAngle)), 
                                                normals[currCount % curvePointsRefined.Count].y,
                                                (-1 * normals[currCount % curvePointsRefined.Count].x * Mathf.Sin(currAngle)) +
                                                (normals[currCount % curvePointsRefined.Count].z * Mathf.Cos(currAngle)));
                }
                UVs[currCount % totalVertices] = new Vector2((float)i /  (float)subdivisions, currHeightLength / heightLength);
                currHeightLength += Vector3.Distance(vertices[j], vertices[j + 1]);
                currCount++;
            }
        }
    }
    public Mesh currMesh;
    private void generateMesh() {
        Debug.Log("creating mesh");
        currMesh = new Mesh();
        currMesh.vertices = vertices;
        currMesh.uv = UVs;
        currMesh.triangles = triangles; 
        currMesh.normals = normals;
        
        GameObject newGameObjectMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newGameObjectMesh.transform.position = transform.position;
        newGameObjectMesh.GetComponent<MeshFilter>().mesh = currMesh;
        newGameObjectMesh.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
    }
    
}