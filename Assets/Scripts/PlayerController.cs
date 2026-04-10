using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseState
{
    public float maxMoveSpeed = 12f;
    public float acceleration = 10f;
    public float jumpVelocityChange = 8f;
    public float alignSpeed = 12f;
    public float runSpeedMultiplier = 1.6f;

    public Rigidbody rb;
    public StateMachine stateMachine;
    public PlayerFall airborneState;
    public PlayerLook playerLook;

    Vector3 moveInput;
    // Vector3 moveAmount;
    bool jumpQueued;
    bool runHeld;
    bool jumpReleasedSinceLastJump = true;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null && rb.collisionDetectionMode == CollisionDetectionMode.Discrete)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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
        if (!Input.GetButton("Jump"))
        {
            jumpReleasedSinceLastJump = true;
        }

        if (jumpReleasedSinceLastJump && Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
            jumpReleasedSinceLastJump = false;
        }

        runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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
        //
        if (airborneState.CurrentAsteroid == null)
        {
            //this is a bad hack, it airborne state is for FALLING, we don't have a free space state, 
            //there is no state for when the player is in the air but not falling, so we just switch to the fall state
            stateMachine.ChangeState(airborneState);
            return;
        }

        if (!airborneState.IsGrounded)
        {
            stateMachine.ChangeState(airborneState);
            return;
        }

        //airborne state NEEDS AN ASTEROID TO FUNCTION RIGHT NOW, need to implement a free space state
        Asteroid asteroid = airborneState.CurrentAsteroid;
        Quaternion targetUpRotation = asteroid.GetTargetUpRotation(rb.rotation, rb.position);
        float effectiveAlignSpeed = alignSpeed * airborneState.AsteroidSwitchAlignMultiplier;
        Quaternion alignedRotation = Quaternion.Slerp(rb.rotation, targetUpRotation, effectiveAlignSpeed * Time.fixedDeltaTime);
        float yawDelta = playerLook != null ? playerLook.ConsumeYawDelta() : 0f;

        if (Mathf.Abs(yawDelta) > 0f)
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, alignedRotation * Vector3.up);
            alignedRotation = yawRotation * alignedRotation;
        }

        rb.MoveRotation(alignedRotation);

        float moveSpeed = maxMoveSpeed * (runHeld ? Mathf.Max(1f, runSpeedMultiplier) : 1f);
        // Vector3 targetMoveAmount = moveInput * moveSpeed;
        // moveAmount = Vector3.Lerp(moveAmount, targetMoveAmount, acceleration * Time.fixedDeltaTime);
        // Vector3 movement = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        // rb.MovePosition(rb.position + movement);
        Vector3 desiredWorld = transform.TransformDirection(moveInput);
        Vector3 tangentInput = Vector3.ProjectOnPlane(desiredWorld, transform.up).normalized;
        Vector3 currentTangentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up);
        Vector3 targetTangentVelocity = tangentInput * moveSpeed;
        Vector3 newTangentVelocity = currentTangentVelocity;
        bool preserveGrappleMomentum = airborneState.IsPreservingGrappleMomentum;
        float preservedSpeed = airborneState.PreservedGrappleSpeed;

        if (preserveGrappleMomentum && currentTangentVelocity.magnitude > moveSpeed && tangentInput.sqrMagnitude < 0.0001f)
        {
            newTangentVelocity = currentTangentVelocity;
        }
        else if (preserveGrappleMomentum && currentTangentVelocity.magnitude > moveSpeed && Vector3.Dot(currentTangentVelocity.normalized, targetTangentVelocity.normalized) > 0.25f)
        {
            Vector3 acceleratedVelocity = Vector3.Lerp(
                currentTangentVelocity,
                targetTangentVelocity.normalized * Mathf.Max(moveSpeed, preservedSpeed),
                acceleration * Time.fixedDeltaTime);
            newTangentVelocity = acceleratedVelocity;
        }
        else
        {
            newTangentVelocity = Vector3.Lerp(
                currentTangentVelocity,
                targetTangentVelocity,
                acceleration * Time.fixedDeltaTime);
        }

        Vector3 upVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        rb.linearVelocity = newTangentVelocity + upVelocity;

        float intoGroundSpeed = Vector3.Dot(rb.linearVelocity, -transform.up);
        if (intoGroundSpeed > 0f)
        {
            rb.linearVelocity += transform.up * intoGroundSpeed;
        }
        if (jumpQueued)
        {
            jumpQueued = false;
            rb.AddForce(transform.up * jumpVelocityChange, ForceMode.VelocityChange);
            airborneState.BeginJumpHold();
            stateMachine.ChangeState(airborneState);
        }
    }
}

