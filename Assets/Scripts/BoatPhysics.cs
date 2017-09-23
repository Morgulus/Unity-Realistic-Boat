using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatPhysics : MonoBehaviour
{
    public GameObject boatMeshObj;
    public GameObject underWaterObj;
    public GameObject aboveWaterObj;

    public Vector3 centerOfMass;

    private ModifyBoatMesh modifyBoatMesh;

    private Mesh underWaterMesh;
    private Mesh aboveWaterMesh;

    private Rigidbody boatRB;

    private float rhoWater = BoatPhysicsMath.RHO_OCEAN_WATER;
    private float rhoAir = BoatPhysicsMath.RHO_AIR;

    void Awake()
    {
        boatRB = this.GetComponent<Rigidbody>();
    }

    void Start()
    {
        modifyBoatMesh = new ModifyBoatMesh(boatMeshObj, underWaterObj, aboveWaterObj, boatRB);

        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
        aboveWaterMesh = aboveWaterObj.GetComponent<MeshFilter>().mesh;
    }

    void Update()
    {
        modifyBoatMesh.GenerateUnderwaterMesh();
        modifyBoatMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyBoatMesh.underWaterTriangleData);
    }

    void FixedUpdate()
    {

        boatRB.centerOfMass = centerOfMass;
        if (modifyBoatMesh.underWaterTriangleData.Count > 0)
            AddUnderWaterForces();
       
        if (modifyBoatMesh.aboveWaterTriangleData.Count > 0)
            AddAboveWaterForces();
    }

    void AddUnderWaterForces()
    {
        float Cf = BoatPhysicsMath.ResistanceCoefficient(
            rhoWater,
            boatRB.velocity.magnitude,
            modifyBoatMesh.CalculateUnderWaterLength());

        List<SlammingForceData> slammingForceData = modifyBoatMesh.slammingForceData;

        CalculateSlammingVelocities(slammingForceData);

        float boatArea = modifyBoatMesh.boatArea;
        float boatMass = boatRB.mass;

        List<int> indexOfOriginalTriangle = modifyBoatMesh.indexOfOriginalTriangle;

        List<TriangleData> underWaterTriangleData = modifyBoatMesh.underWaterTriangleData;

        for (int i = 0; i < underWaterTriangleData.Count; i++)
        {
            TriangleData triangleData = underWaterTriangleData[i];

            Vector3 forceToAdd = Vector3.zero;

            forceToAdd += BoatPhysicsMath.BuoyancyForce(rhoWater, triangleData);

            forceToAdd += BoatPhysicsMath.ViscousWaterResistanceForce(rhoWater, triangleData, Cf);

            forceToAdd += BoatPhysicsMath.PressureDragForce(triangleData);
            int originalTriangleIndex = indexOfOriginalTriangle[i];

            SlammingForceData slammingData = slammingForceData[originalTriangleIndex];
            forceToAdd += BoatPhysicsMath.SlammingForce(slammingData, triangleData, boatArea, boatMass);
            boatRB.AddForceAtPosition(forceToAdd, triangleData.center);
        }
    }

    void AddAboveWaterForces()
    {
        List<TriangleData> aboveWaterTriangleData = modifyBoatMesh.aboveWaterTriangleData;

        for (int i = 0; i < aboveWaterTriangleData.Count; i++)
        {
            TriangleData triangleData = aboveWaterTriangleData[i];
            Vector3 forceToAdd = Vector3.zero;

            forceToAdd += BoatPhysicsMath.AirResistanceForce(rhoAir, triangleData, boatRB.drag);
            boatRB.AddForceAtPosition(forceToAdd, triangleData.center);
        }
    }

    private void CalculateSlammingVelocities(List<SlammingForceData> slammingForceData)
    {
        for (int i = 0; i < slammingForceData.Count; i++)
        {
            slammingForceData[i].previousVelocity = slammingForceData[i].velocity;
            Vector3 center = transform.TransformPoint(slammingForceData[i].triangleCenter);
            slammingForceData[i].velocity = BoatPhysicsMath.GetTriangleVelocity(boatRB, center);
        }
    }
}
