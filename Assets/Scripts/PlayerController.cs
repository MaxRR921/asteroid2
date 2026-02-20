using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseState
{
    public float maxMoveSpeed = 12f;
    public float acceleration = 10f;
    public float jumpVelocityChange = 8f;
    public float alignSpeed = 12f;

    public Rigidbody rb;
    public StateMachine stateMachine;
    public PlayerFall airborneState;
    public PlayerLook playerLook;

    Vector3 moveInput;
    Vector3 moveAmount;
    bool jumpQueued;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (stateMachine == null)
        {
            stateMachine = GetComponent<StateMachine>();
        }

        if (airborneState == null)
        {
            airborneState = GetComponent<PlayerFall>();
        }

        if (playerLook == null)
        {
            playerLook = GetComponent<PlayerLook>();
        }
    }

    public override void stateStart()
    {
    }

    public override void stateUpdate()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }
    }

    public override void stateFixedUpdate()
    {
        if (airborneState == null)
        {
            return;
        }

        if (stateMachine == null)
        {
            return;
        }

        airborneState.RefreshSensors();
        if (airborneState.CurrentAsteroid == null)
        {
            stateMachine.ChangeState(airborneState);
            return;
        }

        if (!airborneState.IsGrounded)
        {
            stateMachine.ChangeState(airborneState);
            return;
        }

        Asteroid asteroid = airborneState.CurrentAsteroid;
        Quaternion targetUpRotation = asteroid.GetTargetUpRotation(rb.rotation, rb.position);
        Quaternion alignedRotation = Quaternion.Slerp(rb.rotation, targetUpRotation, alignSpeed * Time.fixedDeltaTime);
        float yawDelta = playerLook != null ? playerLook.ConsumeYawDelta() : 0f;
        if (Mathf.Abs(yawDelta) > 0f)
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, alignedRotation * Vector3.up);
            alignedRotation = yawRotation * alignedRotation;
        }
        rb.MoveRotation(alignedRotation);

        Vector3 targetMoveAmount = moveInput * maxMoveSpeed;
        moveAmount = Vector3.Lerp(moveAmount, targetMoveAmount, acceleration * Time.fixedDeltaTime);
        Vector3 movement = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (jumpQueued)
        {
            jumpQueued = false;
            stateMachine.ChangeState(airborneState);
            rb.AddForce(transform.up * jumpVelocityChange, ForceMode.VelocityChange);
        }
    }
}
