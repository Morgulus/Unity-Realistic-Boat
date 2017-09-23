using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class EndlessWaterSquare : MonoBehaviour {

    public GameObject boatObj;
    public GameObject waterSqrObj;

    private float squareWidth;
    private float innerSquareResolution;
    private float outerSquareResolution;

    List<WaterSquare> waterSquares = new List<WaterSquare>();

    public float secondsSinceStart;
    public Vector3 boatPos;
    public Vector3 oceanPos;

    public bool hasThreadUpdatedWater;

    private void Start()
    {
        CreateEndlessSea();
        secondsSinceStart = Time.time;
        ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateWaterWithThreadPooling));
        StartCoroutine(UpdateWater());
        
    }

    private void Update()
    {
        secondsSinceStart = Time.time;
        boatPos = boatObj.transform.position;
    }
    
    IEnumerator UpdateWater()
    {
        while (true)
        {
            if (hasThreadUpdatedWater)
            {
                transform.position = oceanPos;
                for (int i = 0; i < waterSquares.Count; i++)
                {
                    waterSquares[i].terrainMeshFilter.mesh.vertices = waterSquares[i].vertices;

                    waterSquares[i].terrainMeshFilter.mesh.RecalculateNormals();
                }

                hasThreadUpdatedWater = false;

                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateWaterWithThreadPooling));
            }
            yield return new WaitForSeconds(Time.deltaTime * 3f);
        }
    }

    void UpdateWaterWithThreadPooling(object state)
    {
        MoveWaterToBoat();
        for (int i = 0; i < waterSquares.Count; i++)
        {
            Vector3 centerPos = waterSquares[i].centerPos;
            Vector3[] vertices = waterSquares[i].vertices;
            for (int j = 0; j < vertices.Length; j++)
            {
                Vector3 vertexPos = vertices[j];
                Vector3 vertexPosGlobal = vertexPos + centerPos + oceanPos;
                vertexPos.y = WaterController.current.GetWaveYPos(vertexPosGlobal, secondsSinceStart);
                vertices[j] = vertexPos;
            }
        }
        hasThreadUpdatedWater = true;
    }

    void MoveWaterToBoat()
    {
        float x = innerSquareResolution * (int)Mathf.Round(boatPos.x / innerSquareResolution);
        float z = innerSquareResolution * (int)Mathf.Round(boatPos.z / innerSquareResolution);
        if (oceanPos.x != x || oceanPos.z != z)
        {
            oceanPos = new Vector3(x, oceanPos.y, z);
        }
    }
    void CreateEndlessSea()
    {
        AddWaterPlane(0f, 0f, 0f, squareWidth, innerSquareResolution);
        for (int x = -1; x <= 1; x += 1)
        {
            for (int z = -1; z <= 1; z += 1)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }
                float yPos = -0.5f;
                AddWaterPlane(x * squareWidth, z * squareWidth, yPos, squareWidth, outerSquareResolution);
            }
        }
    }
    void AddWaterPlane(float xCoord, float zCoord, float yPos, float squareWidth, float spacing)
    {
        GameObject waterPlane = Instantiate(waterSqrObj, transform.position, transform.rotation) as GameObject;

        waterPlane.SetActive(true);
        Vector3 centerPos = transform.position;

        centerPos.x += xCoord;
        centerPos.y = yPos;
        centerPos.z += zCoord;

        waterPlane.transform.position = centerPos;

        waterPlane.transform.parent = transform;

        WaterSquare newWaterSquare = new WaterSquare(waterPlane, squareWidth, spacing);

        waterSquares.Add(newWaterSquare);
    }

}
