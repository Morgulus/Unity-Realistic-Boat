using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSquare {

    public Transform squareTransform;
    public MeshFilter terrainMeshFilter;
    private float size;
    public float spacing;
    private int width;
    public Vector3 centerPos;
    public Vector3[] vertices;

    public WaterSquare(GameObject waterSquareObj, float size, float spacing)
    {
        this.squareTransform = waterSquareObj.transform;
        this.size = size;
        this.spacing = spacing;
        this.terrainMeshFilter = squareTransform.GetComponent<MeshFilter>();
  
        width = (int)(size / spacing);
        width += 1;

        float offset = -((width - 1) * spacing) / 2;
        Vector3 newPos = new Vector3(offset, squareTransform.position.y, offset);
        squareTransform.position += newPos;
        this.centerPos = waterSquareObj.transform.localPosition;

        GenerateMesh();

        this.vertices = terrainMeshFilter.mesh.vertices;
    }

    public void MoveSea(Vector3 oceanPos, float timeSinceStart)
    {
        Vector3[] vertices = terrainMeshFilter.mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 vertexGlobal = vertex + centerPos + oceanPos;
            vertex.y = WaterController.current.GetWaveYPos(vertexGlobal, timeSinceStart);
            vertices[i] = vertex;
        }
        terrainMeshFilter.mesh.vertices = vertices;
        terrainMeshFilter.mesh.RecalculateNormals();
    }

    public void GenerateMesh()
    {
        List<Vector3[]> verts = new List<Vector3[]>();
        List<int> tris = new List<int>();

        for (int z = 0; z < width; z++)
        {
            verts.Add(new Vector3[width]);
            for (int x = 0; x < width; x++)
            {
                Vector3 current_point = new Vector3();

                current_point.x = x * spacing;
                current_point.z = z * spacing;
                current_point.y = squareTransform.position.y;
                verts[z][x] = current_point;

                if (x <= 0 || z <= 0)
                {
                    continue;
                }

                tris.Add(x + z * width);
                tris.Add(x + (z - 1) * width);
                tris.Add((x - 1) + (z - 1) * width);

                tris.Add(x + z * width);
                tris.Add((x - 1) + (z - 1) * width);
                tris.Add((x - 1) + z * width);
            }
        }

        Vector3[] unfolded_verts = new Vector3[width * width];

        int i = 0;
        foreach (Vector3[] v in verts)
        {
            v.CopyTo(unfolded_verts, i * width);
            i++;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = unfolded_verts;
        newMesh.triangles = tris.ToArray();

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        terrainMeshFilter.mesh.Clear();
        terrainMeshFilter.mesh = newMesh;
        terrainMeshFilter.mesh.name = "Water Mesh";
    }
}
