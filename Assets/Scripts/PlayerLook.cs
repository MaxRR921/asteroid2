using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float senseX = 250f;
    public float senseY = 250f;
    public Camera playerCamera;
    public float thirdPersonDistance = 3f;
    public float cameraHeight = 1.6f;
    public float upSmoothing = 12f;

    Transform cameraTransform;
    Rigidbody rb;
    float verticalLookRotation;
    float yawDeltaAccumulator;
    Vector3 smoothedUp;
    Vector3 orbitForward;
    bool orbitInitialized;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }

        smoothedUp = transform.up;
        Vector3 initialForward = Vector3.ProjectOnPlane(transform.forward, smoothedUp);
        if (initialForward.sqrMagnitude < 0.0001f && cameraTransform != null)
        {
            initialForward = Vector3.ProjectOnPlane(cameraTransform.forward, smoothedUp);
        }
        if (initialForward.sqrMagnitude < 0.0001f)
        {
            initialForward = Vector3.forward;
        }
        orbitForward = initialForward.normalized;
        orbitInitialized = true;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float yawDelta = Input.GetAxis("Mouse X") * Time.deltaTime * senseX;
        yawDeltaAccumulator += yawDelta;

        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * senseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);

        if (orbitInitialized)
        {
            orbitForward = Quaternion.AngleAxis(yawDelta, smoothedUp) * orbitForward;
            orbitForward = Vector3.ProjectOnPlane(orbitForward, smoothedUp);
            if (orbitForward.sqrMagnitude < 0.0001f)
            {
                orbitForward = Vector3.ProjectOnPlane(transform.forward, smoothedUp);
            }
            orbitForward = orbitForward.normalized;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            float upBlend = 1f - Mathf.Exp(-Mathf.Max(0.001f, upSmoothing) * Time.deltaTime);
            smoothedUp = Vector3.Slerp(smoothedUp, transform.up, upBlend).normalized;

            Vector3 planarForward = Vector3.ProjectOnPlane(orbitForward, smoothedUp);
            if (planarForward.sqrMagnitude < 0.0001f)
            {
                planarForward = Vector3.ProjectOnPlane(transform.forward, smoothedUp);
            }
            planarForward = planarForward.normalized;

            Vector3 right = Vector3.Cross(smoothedUp, planarForward).normalized;
            Quaternion pitchRotation = Quaternion.AngleAxis(-verticalLookRotation, right);
            Vector3 cameraDirection = pitchRotation * (-planarForward);
            Vector3 focusPoint = transform.position + (smoothedUp * cameraHeight);
            Vector3 cameraPosition = focusPoint + (cameraDirection * thirdPersonDistance);

            cameraTransform.position = cameraPosition;
            cameraTransform.rotation = Quaternion.LookRotation((focusPoint - cameraPosition).normalized, smoothedUp);
        }
    }

    public float ConsumeYawDelta()
    {
        float yawDelta = yawDeltaAccumulator;
        yawDeltaAccumulator = 0f;
        return yawDelta;
    }
}
