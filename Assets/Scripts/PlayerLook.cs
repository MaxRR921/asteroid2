using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float senseX = 250f;
    public float senseY = 250f;
    public Camera playerCamera;

    Transform cameraTransform;
    Rigidbody rb;
    float verticalLookRotation;
    float yawDeltaAccumulator;

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
        yawDeltaAccumulator += Input.GetAxis("Mouse X") * Time.deltaTime * senseX;

        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * senseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
    }

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;
        }
    }

    public float ConsumeYawDelta()
    {
        float yawDelta = yawDeltaAccumulator;
        yawDeltaAccumulator = 0f;
        return yawDelta;
    }
}
