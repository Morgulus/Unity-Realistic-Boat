using UnityEngine;
using System.Collections;

public static class BoatPhysicsMath
{

    public const float RHO_WATER = 1000f;
    public const float RHO_OCEAN_WATER = 1027f;
    public const float RHO_SUNFLOWER_OIL = 920f;
    public const float RHO_MILK = 1035f;

    public const float RHO_AIR = 1.225f;
    public const float RHO_HELIUM = 0.164f;

    public const float RHO_GOLD = 19300f;

    public const float C_D_FLAT_PLATE_PERPENDICULAR_TO_FLOW = 1.28f;

    public static Vector3 GetTriangleVelocity(Rigidbody boatRB, Vector3 triangleCenter)
    {
        Vector3 v_B = boatRB.velocity;
        Vector3 omega_B = boatRB.angularVelocity;
        Vector3 r_BA = triangleCenter - boatRB.worldCenterOfMass;
        Vector3 v_A = v_B + Vector3.Cross(omega_B, r_BA);
        return v_A;
    }

    public static float GetTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Vector3.Distance(p1, p2);
        float c = Vector3.Distance(p3, p1);
        float areaSin = (a * c * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad)) / 2f;
        float area = areaSin;
        return area;
    }

    public static Vector3 BuoyancyForce(float rho, TriangleData triangleData)
    {
        Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        buoyancyForce = CheckForceIsValid(buoyancyForce, "Buoyancy");

        return buoyancyForce;
    }

    public static Vector3 ViscousWaterResistanceForce(float rho, TriangleData triangleData, float Cf)
    {
    
        Vector3 B = triangleData.normal;
        Vector3 A = triangleData.velocity;

        Vector3 velocityTangent = Vector3.Cross(B, (Vector3.Cross(A, B) / B.magnitude)) / B.magnitude;
        Vector3 tangentialDirection = velocityTangent.normalized * -1f;

        Vector3 v_f_vec = triangleData.velocity.magnitude * tangentialDirection;

        Vector3 viscousWaterResistanceForce = 0.5f * rho * v_f_vec.magnitude * v_f_vec * triangleData.area * Cf;

        viscousWaterResistanceForce = CheckForceIsValid(viscousWaterResistanceForce, "Viscous Water Resistance");

        return viscousWaterResistanceForce;
    }
    
    public static float ResistanceCoefficient(float rho, float velocity, float length)
    {

        float nu = 0.000001f;
        float Rn = (velocity * length) / nu;
        float Cf = 0.075f / Mathf.Pow((Mathf.Log10(Rn) - 2f), 2f);
        return Cf;
    }

    public static Vector3 PressureDragForce(TriangleData triangleData)
    {
        float velocity = triangleData.velocity.magnitude;
        float velocityReference = velocity;
        velocity = velocity / velocityReference;
        Vector3 pressureDragForce = Vector3.zero;

        if (triangleData.cosTheta > 0f)
        {
            float C_PD1 = DebugPhysics.current.C_PD1;
            float C_PD2 = DebugPhysics.current.C_PD2;
            float f_P = DebugPhysics.current.f_P;

            pressureDragForce = -(C_PD1 * velocity + C_PD2 * (velocity * velocity)) * triangleData.area * Mathf.Pow(triangleData.cosTheta, f_P) * triangleData.normal;
        }
        else
        {
            float C_SD1 = DebugPhysics.current.C_SD1;
            float C_SD2 = DebugPhysics.current.C_SD2;
            float f_S = DebugPhysics.current.f_S;

            pressureDragForce = (C_SD1 * velocity + C_SD2 * (velocity * velocity)) * triangleData.area * Mathf.Pow(Mathf.Abs(triangleData.cosTheta), f_S) * triangleData.normal;
        }

        pressureDragForce = CheckForceIsValid(pressureDragForce, "Pressure drag");

        return pressureDragForce;
    }

    public static Vector3 SlammingForce(SlammingForceData slammingData, TriangleData triangleData, float boatArea, float boatMass)
    {
        if (triangleData.cosTheta < 0f || slammingData.originalArea <= 0f)
        {
            return Vector3.zero;
        }

        Vector3 dV = slammingData.submergedArea * slammingData.velocity;
        Vector3 dV_previous = slammingData.previousSubmergedArea * slammingData.previousVelocity;

        Vector3 accVec = (dV - dV_previous) / (slammingData.originalArea * Time.fixedDeltaTime);

        float acc = accVec.magnitude;

        Vector3 F_stop = boatMass * triangleData.velocity * ((2f * triangleData.area) / boatArea);
        float p = 2f;
        float acc_max = acc;
        float slammingCheat = DebugPhysics.current.slammingCheat;
        Vector3 slammingForce = Mathf.Pow(Mathf.Clamp01(acc / acc_max), p) * triangleData.cosTheta * F_stop * slammingCheat;

        slammingForce *= -1f;
        slammingForce = CheckForceIsValid(slammingForce, "Slamming");
        return slammingForce;
    }

    public static float ResidualResistanceForce()
    { 
        float residualResistanceForce = 0f;
        return residualResistanceForce;
    }

    public static Vector3 AirResistanceForce(float rho, TriangleData triangleData, float C_air)
    {
        if (triangleData.cosTheta < 0f)
        {
            return Vector3.zero;
        }
        Vector3 airResistanceForce = 0.5f * rho * triangleData.velocity.magnitude * triangleData.velocity * triangleData.area * C_air;
        airResistanceForce *= -1f;
        airResistanceForce = CheckForceIsValid(airResistanceForce, "Air resistance");
        return airResistanceForce;
    }

    private static Vector3 CheckForceIsValid(Vector3 force, string forceName)
    {
        if (!float.IsNaN(force.x + force.y + force.z))
        {
            return force;
        }
        else
        {
            Debug.Log(forceName += " force is NaN");

            return Vector3.zero;
        }
    }
}