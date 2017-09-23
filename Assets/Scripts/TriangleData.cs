using UnityEngine;
using System.Collections;

public struct TriangleData
{
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public Vector3 center;
    public Vector3 normal;

    public float distanceToSurface;
    public float area;

    public Vector3 velocity;
    public Vector3 velocityDir;

    public float cosTheta;

    public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3, Rigidbody boatRB, float timeSinceStart)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.center = (p1 + p2 + p3) / 3f;
        this.distanceToSurface = Mathf.Abs(WaterController.current.DistanceToWater(this.center, timeSinceStart));
        this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        this.area = BoatPhysicsMath.GetTriangleArea(p1, p2, p3);
        this.velocity = BoatPhysicsMath.GetTriangleVelocity(boatRB, this.center);
        this.velocityDir = this.velocity.normalized;
        this.cosTheta = Vector3.Dot(this.velocityDir, this.normal);
    }
}
