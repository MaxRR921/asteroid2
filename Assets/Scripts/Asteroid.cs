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
    public MeshFilter deformMeshFilter;
    public MeshCollider deformMeshCollider;
    public bool updateColliderOnImpact = true;
    public float defaultImpactRadius = 1.2f;
    public float defaultImpactDepth = 0.45f;
    public float defaultImpactFalloff = 2.5f;

    private Mesh runtimeMesh;
    private Vector3[] deformedVertices;
    private bool canDeform;

    void Awake()
    {
        if (deformMeshFilter == null)
        {
            deformMeshFilter = GetComponent<MeshFilter>();
        }

        if (deformMeshFilter == null)
        {
            deformMeshFilter = GetComponentInChildren<MeshFilter>();
        }

        if (deformMeshCollider == null)
        {
            deformMeshCollider = GetComponent<MeshCollider>();
        }

        if (deformMeshCollider == null)
        {
            deformMeshCollider = GetComponentInChildren<MeshCollider>();
        }

        InitializeDeformMesh();
    }

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

    public bool DeformAtPoint(Vector3 worldPoint, Vector3 worldNormal, float radius, float depth, float falloff)
    {
        if (!canDeform || runtimeMesh == null || deformedVertices == null || deformedVertices.Length == 0)
        {
            return false;
        }

        float safeRadius = Mathf.Max(0.01f, radius);
        float safeDepth = Mathf.Max(0f, depth);
        float safeFalloff = Mathf.Max(0.01f, falloff);
        Vector3 localPoint = deformMeshFilter.transform.InverseTransformPoint(worldPoint);
        Vector3 localNormal = deformMeshFilter.transform.InverseTransformDirection(worldNormal).normalized;
        bool changed = false;

        for (int i = 0; i < deformedVertices.Length; i++)
        {
            Vector3 vertex = deformedVertices[i];
            float distance = Vector3.Distance(vertex, localPoint);
            if (distance > safeRadius)
            {
                continue;
            }

            float normalizedDistance = distance / safeRadius;
            float dentStrength = Mathf.Pow(1f - normalizedDistance, safeFalloff) * safeDepth;
            deformedVertices[i] = vertex - (localNormal * dentStrength);
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        runtimeMesh.vertices = deformedVertices;
        runtimeMesh.RecalculateNormals();
        runtimeMesh.RecalculateBounds();

        if (updateColliderOnImpact && deformMeshCollider != null)
        {
            deformMeshCollider.sharedMesh = null;
            deformMeshCollider.sharedMesh = runtimeMesh;
        }

        return true;
    }

    public bool DeformAtPoint(Vector3 worldPoint, Vector3 worldNormal)
    {
        return DeformAtPoint(worldPoint, worldNormal, defaultImpactRadius, defaultImpactDepth, defaultImpactFalloff);
    }

    private void InitializeDeformMesh()
    {
        if (deformMeshFilter == null || deformMeshFilter.sharedMesh == null)
        {
            canDeform = false;
            return;
        }

        runtimeMesh = Instantiate(deformMeshFilter.sharedMesh);
        runtimeMesh.name = deformMeshFilter.sharedMesh.name + " (Runtime Deformed)";
        deformMeshFilter.mesh = runtimeMesh;
        deformedVertices = runtimeMesh.vertices;
        canDeform = true;

        if (deformMeshCollider != null)
        {
            deformMeshCollider.sharedMesh = runtimeMesh;
        }
    }

    // public void AlignBody(Rigidbody body, float alignSpeed)
    // {
    //     Quaternion targetRotation = GetTargetUpRotation(body.rotation, body.position);
    //     Quaternion smoothed = Quaternion.Slerp(body.rotation, targetRotation, alignSpeed * Time.fixedDeltaTime);
    //     body.MoveRotation(smoothed);
    // }
}
