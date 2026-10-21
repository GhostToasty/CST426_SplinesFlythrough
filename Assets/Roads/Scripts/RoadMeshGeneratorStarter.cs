using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System.Linq;

public class RoadMeshGeneratorStarter : MonoBehaviour
{
    public SplinePath splinePath;
    public int rungs = 10;
    public float width = 3f;
    public Material material;
    public float textureRepeatLength = 2f;
    public bool rebuildEveryFrame;
    public float[] bankAngles;
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    float depth = 3f;


    void Start()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        BuildRoadMesh();
    }


    void Update()
    {
        if (rebuildEveryFrame)
            BuildRoadMesh();
    }


    void BuildRoadMesh()
    {
        List<Vector3> verts = new();
        List<Vector3> lowerVerts = new();
        List<Vector2> uvs = new();
        List<Vector2> lowerUvs = new();
        Array.Resize(ref bankAngles, splinePath.SegmentCount + 1);

        for (int i = 0; i < rungs; i++)
        {
            //get distance to the rung
            float fraction = i / (float)(rungs - 1);
            float distance = splinePath.TotalLength * fraction;

            //use distance to look up u
            float u = splinePath.ParameterAtDistance(distance);

            //use u to get center point of rung
            Vector3 centerPoint = splinePath.SamplePoint(u);
            Vector3 tangent = splinePath.SampleTangent(u);
            
            int segment = (int)Mathf.Min(u, splinePath.SegmentCount - 1);
            float t = u - segment;
            float bankAngle = Mathf.SmoothStep(bankAngles[segment], bankAngles[segment + 1], t);

            //calculate "right" direction 
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            right = Quaternion.AngleAxis(bankAngle, tangent) * right;
            
            float halfWidth = width * 0.5f;
            float lift = Mathf.Sin(bankAngle * Mathf.Deg2Rad) * halfWidth;
            
            centerPoint += Vector3.up * Mathf.Abs(lift);

            //create left and right verts for top and bottom of road
            Vector3 leftVert = centerPoint - right * halfWidth;
            Vector3 rightVert = centerPoint + right * halfWidth;
            Vector3 lowerLeftVert = leftVert - Vector3.up * depth;
            Vector3 lowerRightVert = rightVert - Vector3.up * depth;

            verts.Add(leftVert);
            verts.Add(rightVert);
            lowerVerts.Add(lowerLeftVert);
            lowerVerts.Add(lowerRightVert);

            uvs.Add(new Vector2 (0f, distance / textureRepeatLength));
            uvs.Add(new Vector2 (1f, distance / textureRepeatLength));
            lowerUvs.Add(new Vector2 (0f, distance / textureRepeatLength));
            lowerUvs.Add(new Vector2 (1f, distance / textureRepeatLength));

        }

        //combines regular and lower verts and uvs into one array, respectively 
        List<Vector3> combinedVerts = verts.Concat(lowerVerts).ToList();
        List<Vector2> combinedUvs = uvs.Concat(lowerUvs).ToList();
        

        //calculate triangles
        List<int> triangles = new();
        for(int i = 0; i < rungs - 1; i++)
        {
            int lowerOffset = verts.Count;

            int bottomLeft = i * 2;
            int bottomRight = bottomLeft + 1;
            int topLeft = (i + 1) * 2;
            int topRight = topLeft + 1;

            int lowerBottomLeft = lowerOffset + bottomLeft;
            int lowerBottomRight = lowerOffset + bottomRight;
            int lowerTopLeft = lowerOffset + topLeft;
            int lowerTopRight = lowerOffset + topRight;

            //top road
            triangles.Add(bottomLeft);
            triangles.Add(topLeft);
            triangles.Add(bottomRight);

            triangles.Add(topLeft);
            triangles.Add(topRight);
            triangles.Add(bottomRight);

            //left side
            triangles.Add(lowerTopLeft);
            triangles.Add(topLeft);
            triangles.Add(lowerBottomLeft);

            triangles.Add(topLeft);
            triangles.Add(bottomLeft);
            triangles.Add(lowerBottomLeft);

            //right side
            triangles.Add(lowerBottomRight);
            triangles.Add(bottomRight);
            triangles.Add(lowerTopRight);

            triangles.Add(bottomRight);
            triangles.Add(topRight);
            triangles.Add(lowerTopRight);

            //bottom road
            triangles.Add(lowerTopLeft);
            triangles.Add(lowerBottomLeft);
            triangles.Add(lowerTopRight);

            triangles.Add(lowerBottomLeft);
            triangles.Add(lowerBottomRight);
            triangles.Add(lowerTopRight);

            //first end
            if (i == 0)
            {
                triangles.Add(lowerBottomLeft);
                triangles.Add(bottomLeft);
                triangles.Add(lowerBottomRight);

                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
                triangles.Add(lowerBottomRight);
            }

            //second end
            if (i == rungs - 2)
            {
                triangles.Add(lowerTopRight);
                triangles.Add(topRight);
                triangles.Add(lowerTopLeft);

                triangles.Add(topRight);
                triangles.Add(topLeft);
                triangles.Add(lowerTopLeft);
            }
            
        }

        //convert triangles into mesh
        Mesh mesh = new();
        mesh.vertices = combinedVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = combinedUvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        _meshFilter.mesh = mesh;
        _meshRenderer.material = material;

    }
}