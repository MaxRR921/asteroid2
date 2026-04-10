using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public enum GravityMode
    {
        Constant,
        InverseSquare
    }

    public GravityMode gravityMode = GravityMode.Constant;
    public float gravityStrength = 30f;
    public float minGravityDistance = 1f;

    public Vector3 GetSurfaceUp(Vector3 worldPosition)
    {
        return (worldPosition - transform.position).normalized;
    }

    public Vector3 GetGravityAcceleration(Vector3 worldPosition)
    {
        Vector3 toCenter = transform.position - worldPosition;
        float distance = Mathf.Max(toCenter.magnitude, minGravityDistance);

        if (gravityMode == GravityMode.InverseSquare)
        {
            return toCenter.normalized * (gravityStrength / (distance * distance));
        }

        return toCenter.normalized * gravityStrength;
    }

    public Quaternion GetTargetUpRotation(Quaternion currentRotation, Vector3 worldPosition)
    {
        Vector3 bodyUp = currentRotation * Vector3.up;
        Vector3 targetUp = GetSurfaceUp(worldPosition);
        return Quaternion.FromToRotation(bodyUp, targetUp) * currentRotation;
    }

    // public void AlignBody(Rigidbody body, float alignSpeed)
    // {
    //     Quaternion targetRotation = GetTargetUpRotation(body.rotation, body.position);
    //     Quaternion smoothed = Quaternion.Slerp(body.rotation, targetRotation, alignSpeed * Time.fixedDeltaTime);
    //     body.MoveRotation(smoothed);
    // }
}
