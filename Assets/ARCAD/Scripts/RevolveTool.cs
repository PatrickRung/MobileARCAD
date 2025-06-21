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
        Vector2 objectScreenPos = new Vector2(Camera.main.WorldToScreenPoint(gameObject.transform.position).x,
                                                Camera.main.WorldToScreenPoint(gameObject.transform.position).y);
        extrudeLength = Vector2.Distance(pointerPos, objectScreenPos);
        extrudeLength = -Mathf.Abs(extrudeLength / 100);
        finishExtrude();

    }
    public void useToggleExtrudeTool()
    {

        extrudeLength = playerCam.transform.InverseTransformPoint(gameObject.transform.position).z;
        Debug.Log(playerCam.transform.InverseTransformPoint(gameObject.transform.position).z);
        extrudeLength = -Mathf.Max(Mathf.Abs(extrudeLength * 100) - 3, 0);
        finishExtrude();

    }
    public void finishExtrude()
    {
        if (curvePoints.Count > 4)
        {
            curvePoints.Sort((element1, element2) => element2.y.CompareTo(element1.y));
            curvePoints.Add(curvePoints[0]);
            int totalVertices = curvePoints.Count * 2;
            vertices = new Vector3[totalVertices];
            normals = new Vector3[totalVertices];
            UVs = new Vector2[totalVertices];
            triangles = new int[((curvePoints.Count - 1) * 6) + ((curvePoints.Count - 2) * 3)];
            int currTriangle = 0;
            for (int i = 0; i < curvePoints.Count; i++)
            {
                vertices[i] = new Vector3(curvePoints[i].x,
                curvePoints[i].y, 0);
                vertices[i + curvePoints.Count] = new Vector3(curvePoints[i].x,
                curvePoints[i].y, extrudeLength / 10f);

                if (i != curvePoints.Count - 1)
                {
                    triangles[currTriangle] = i;
                    triangles[currTriangle + 1] = (i + 1) % totalVertices;
                    triangles[currTriangle + 2] = (i + curvePoints.Count) % totalVertices;
                    currTriangle += 3;
                    // Make triangle 2 for surface
                    triangles[currTriangle] = (i + curvePoints.Count) % totalVertices;
                    triangles[currTriangle + 1] = (i + 1) % totalVertices;
                    triangles[currTriangle + 2] = (i + curvePoints.Count + 1) % totalVertices;
                    currTriangle += 3;
                }
            }
            // Create caps
            for (int i = 0; i < curvePoints.Count - 2; i++)
            {
                if (i != curvePoints.Count - 1)
                {
                    int baseZeroCount = curvePoints.Count - 1;
                    // triangles[currTriangle] = i;
                    // triangles[currTriangle + 1] = curvePoints.Count;
                    // triangles[currTriangle + 2] = i + 1;
                    // currTriangle += 3;
                    triangles[currTriangle + 2] = i + baseZeroCount;
                    triangles[currTriangle + 1] = baseZeroCount + baseZeroCount;
                    triangles[currTriangle] = i + 1 + baseZeroCount;
                    currTriangle += 3;
                }
            }
            // calculate normals
            int currCount = 0;
            for (int j = 0; j < curvePoints.Count; j++)
            {
                // Calculate normals for not last row
                // Vector3 currTangVec = new Vector3(1, 0, 0);
                // Vector3 diffVec;
                // if (j == 0)
                // {
                //     diffVec = vertices[curvePoints.Count] - vertices[currCount];
                // }
                // else if (j == curvePoints.Count - 1)
                // {
                //     diffVec = vertices[currCount] - vertices[currCount - 1];
                // }
                // else
                // {
                //     diffVec = vertices[currCount + 1] - vertices[currCount - 1];
                // }
                // normals[currCount] = Vector3.Cross(diffVec, currTangVec);
                // float mag = Mathf.Sqrt(Mathf.Pow(normals[currCount].x, 2) + Mathf.Pow(normals[currCount].y, 2) + Mathf.Pow(normals[currCount].z, 2));
                // normals[currCount] = new Vector3(normals[currCount].x / mag, normals[currCount].y / mag, normals[currCount].z / mag);
                // normals[currCount + curvePoints.Count] = normals[currCount];
                Vector3 diffVec = vertices[currCount + curvePoints.Count] - vertices[currCount];
                if (j == 0)
                {
                    Vector3 towardsOrigVec = vertices[currCount + curvePoints.Count - 1] - vertices[currCount];
                    Vector3 awayOriVec = vertices[currCount + 1] - vertices[currCount];
                    normals[currCount] = Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                    towardsOrigVec = vertices[currCount + (curvePoints.Count * 2)  - 1] - vertices[currCount + curvePoints.Count];
                    awayOriVec = vertices[currCount + curvePoints.Count + 1] - vertices[currCount + curvePoints.Count];
                    diffVec = -diffVec;
                    normals[currCount + curvePoints.Count] = -Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                }
                else if (j == curvePoints.Count - 1)
                {
                    Vector3 towardsOrigVec = vertices[currCount - 1] - vertices[currCount];
                    Vector3 awayOriVec = vertices[currCount - (curvePoints.Count - 1)] - vertices[currCount];
                    normals[currCount] = Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                    towardsOrigVec = vertices[currCount + curvePoints.Count - 1] - vertices[currCount + curvePoints.Count];
                    awayOriVec = vertices[currCount + curvePoints.Count - (curvePoints.Count - 1)] - vertices[currCount + (curvePoints.Count / 2)];
                    diffVec = -diffVec;
                    normals[currCount + curvePoints.Count] = -Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                }
                else
                {
                    Vector3 towardsOrigVec = vertices[currCount - 1] - vertices[currCount];
                    Vector3 awayOriVec = vertices[currCount + 1] - vertices[currCount];
                    normals[currCount] = Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                    towardsOrigVec = vertices[currCount + curvePoints.Count - 1] - vertices[currCount + curvePoints.Count];
                    awayOriVec = vertices[currCount + curvePoints.Count + 1] - vertices[currCount + curvePoints.Count];
                    diffVec = -diffVec;
                    normals[currCount + curvePoints.Count] = -Vector3.Normalize(diffVec + towardsOrigVec + awayOriVec);
                }
                currCount++;
            }
            generateMesh();
            if (curvePoints.Count <= 0)
            {
                curvePoints.RemoveAt(curvePoints.Count);
            }
        }
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
    public GameObject currDesign;
    private void generateMesh() {
        Debug.Log("creating mesh");
        currMesh = new Mesh();
        currMesh.vertices = vertices;
        currMesh.uv = UVs;
        currMesh.triangles = triangles; 
        currMesh.normals = normals;
        
        if(currDesign == null) {
            currDesign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currDesign.transform.position = transform.position;
        }
        currDesign.GetComponent<MeshFilter>().mesh = currMesh;
        currDesign.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().material;
    }
    
}