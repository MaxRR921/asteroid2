using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseState
{
    public float senseX = 250f;
    public float senseY = 250f;
    public float moveSpeed = 5f;
    public float jumpForce = 450f;

    public Rigidbody rb;
    public Camera camera;
    Transform cameraT;
    float verticalLookRotation;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    public override void stateStart()
    {
        cameraT = camera.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    public override void stateUpdate()
    {
        Debug.Log("Update Runs");
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * senseX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * senseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);
        cameraT.localEulerAngles = Vector3.left * verticalLookRotation;

        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0 , Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMoveAmount = moveDir * moveSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        if (Input.GetButtonDown("Jump"))
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

}
