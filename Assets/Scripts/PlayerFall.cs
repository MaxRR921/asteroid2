using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFall : BaseState
{
    public Rigidbody rb;
    public StateMachine stateMachine;
    public BaseState groundedState;
    public PlayerLook playerLook;

    public float alignSpeed = 10f;
    public float asteroidSwitchAlignMultiplier = 0.2f;
    public float asteroidSwitchSlowDuration = 0.75f;
    public float airControlAcceleration = 20f;
    public float maxAirSpeed = 40f;
    public float maxGroundApproachSpeed = 45f;
    public float runSpeedMultiplier = 1.35f;
    public float airJumpVelocityChange = 7f;
    public int maxAirJumps = 1;
    public float airDashVelocityChange = 14f;
    public float airDashCooldown = 0.2f;
    public int maxAirDashes = 1;
    public float jumpHoldAcceleration = 28f;
    public float jumpHoldMaxTime = 0.2f;
    public float groundCheckDistance = 20f;
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    Asteroid currentAsteroid;
    Vector3 airInput;
    bool isGrounded;
    float asteroidSwitchSlowTimer;
    float jumpHoldTimer;
    bool jumpHoldActive;
    int remainingAirJumps;
    bool airJumpQueued;
    bool runHeld;
    int remainingAirDashes;
    bool airDashQueued;
    float airDashCooldownTimer;
    bool jumpReleasedSinceLastJump = true;
    float grappleSpeedPreserveTimer;
    float preservedGrappleSpeed;
    Vector3 preservedGrappleVelocity;

    public Asteroid CurrentAsteroid
    {
        get { return currentAsteroid; }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    public float AsteroidSwitchAlignMultiplier
    {
        get
        {
            if (asteroidSwitchSlowTimer > 0f)
            {
                return Mathf.Max(0f, asteroidSwitchAlignMultiplier);
            }
            return 1f;
        }
    }

    public bool IsPreservingGrappleMomentum
    {
        get { return grappleSpeedPreserveTimer > 0f; }
    }

    public float PreservedGrappleSpeed
    {
        get { return preservedGrappleSpeed; }
    }

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

        if (playerLook == null)
        {
            playerLook = GetComponent<PlayerLook>();
        }
    }

    public override void stateUpdate()
    {
        airInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        if (!Input.GetButton("Jump"))
        {
            jumpReleasedSinceLastJump = true;
        }

        if (jumpReleasedSinceLastJump && Input.GetButtonDown("Jump"))
        {
            airJumpQueued = true;
            jumpReleasedSinceLastJump = false;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            airDashQueued = true;
        }

        runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public override void stateStart()
    {
        remainingAirJumps = Mathf.Max(0, maxAirJumps);
        remainingAirDashes = Mathf.Max(0, maxAirDashes);
        airJumpQueued = false;
        airDashQueued = false;
        airDashCooldownTimer = 0f;
    }

    public override void stateFixedUpdate()
    {
        if (airDashCooldownTimer > 0f)
        {
            airDashCooldownTimer -= Time.fixedDeltaTime;
        }

        if (grappleSpeedPreserveTimer > 0f)
        {
            grappleSpeedPreserveTimer -= Time.fixedDeltaTime;
            if (grappleSpeedPreserveTimer <= 0f)
            {
                grappleSpeedPreserveTimer = 0f;
                preservedGrappleSpeed = 0f;
                preservedGrappleVelocity = Vector3.zero;
            }
        }

        if (asteroidSwitchSlowTimer > 0f)
        {
            asteroidSwitchSlowTimer -= Time.fixedDeltaTime;
        }

        RefreshSensors();

        if (currentAsteroid == null)
        {
            return;
        }

        Quaternion targetUpRotation = currentAsteroid.GetTargetUpRotation(rb.rotation, rb.position);
        float effectiveAlignSpeed = alignSpeed * AsteroidSwitchAlignMultiplier;
        Quaternion alignedRotation = Quaternion.Slerp(rb.rotation, targetUpRotation, effectiveAlignSpeed * Time.fixedDeltaTime);
        float yawDelta = playerLook != null ? playerLook.ConsumeYawDelta() : 0f;
        if (Mathf.Abs(yawDelta) > 0f)
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, alignedRotation * Vector3.up);
            alignedRotation = yawRotation * alignedRotation;
        }
        rb.MoveRotation(alignedRotation);
        Vector3 gravityAcceleration = currentAsteroid.GetGravityAcceleration(rb.position);
        rb.AddForce(gravityAcceleration, ForceMode.Acceleration);

        if (airJumpQueued)
        {
            airJumpQueued = false;
            if (remainingAirJumps > 0)
            {
                remainingAirJumps--;
                rb.AddForce(transform.up * airJumpVelocityChange, ForceMode.VelocityChange);
                BeginJumpHold();
            }
        }

        if (jumpHoldActive)
        {
            if (jumpHoldTimer > 0f && Input.GetButton("Jump"))
            {
                rb.AddForce(transform.up * jumpHoldAcceleration, ForceMode.Acceleration);
                jumpHoldTimer -= Time.fixedDeltaTime;
            }
            else
            {
                jumpHoldActive = false;
                jumpHoldTimer = 0f;
            }
        }

        if (airDashQueued)
        {
            airDashQueued = false;
            if (remainingAirDashes > 0 && airDashCooldownTimer <= 0f)
            {
                Vector3 dashInputWorld = transform.TransformDirection(airInput);
                Vector3 dashDirection = Vector3.ProjectOnPlane(dashInputWorld, transform.up).normalized;
                if (dashDirection.sqrMagnitude > 0.0001f)
                {
                    remainingAirDashes--;
                    airDashCooldownTimer = Mathf.Max(0f, airDashCooldown);
                    rb.AddForce(dashDirection * airDashVelocityChange, ForceMode.VelocityChange);
                }
            }
        }

        Vector3 desiredWorld = transform.TransformDirection(airInput);
        Vector3 tangentInput = Vector3.ProjectOnPlane(desiredWorld, transform.up).normalized;
        float airSpeedMultiplier = runHeld ? Mathf.Max(1f, runSpeedMultiplier) : 1f;
        if (!(IsPreservingGrappleMomentum && tangentInput.sqrMagnitude < 0.0001f))
        {
            rb.AddForce(tangentInput * airControlAcceleration * airSpeedMultiplier, ForceMode.Acceleration);
        }

        Vector3 upVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        Vector3 tangentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up);
        float allowedAirSpeed = maxAirSpeed * airSpeedMultiplier;
        if (IsPreservingGrappleMomentum)
        {
            allowedAirSpeed = Mathf.Max(allowedAirSpeed, preservedGrappleSpeed);
        }

        if (tangentVelocity.magnitude > allowedAirSpeed)
        {
            tangentVelocity = tangentVelocity.normalized * allowedAirSpeed;
            rb.linearVelocity = tangentVelocity + upVelocity;
        }

        if (IsPreservingGrappleMomentum)
        {
            Vector3 preservedTangentVelocity = Vector3.ProjectOnPlane(preservedGrappleVelocity, transform.up);
            float preservedTangentSpeed = preservedTangentVelocity.magnitude;
            Vector3 currentTangentAfterClamp = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up);

            if (preservedTangentSpeed > 0.01f && currentTangentAfterClamp.magnitude < preservedTangentSpeed)
            {
                Vector3 preservedDirection = preservedTangentVelocity.normalized;
                float steerAlignment = tangentInput.sqrMagnitude > 0.0001f ? Vector3.Dot(tangentInput, preservedDirection) : 1f;

                if (steerAlignment > -0.15f)
                {
                    rb.linearVelocity += preservedDirection * (preservedTangentSpeed - currentTangentAfterClamp.magnitude);
                }
            }
        }

        Debug.Log("HELLO");
        float approachSpeed = Vector3.Dot(rb.linearVelocity, -transform.up);
        if (approachSpeed > maxGroundApproachSpeed)
        {
            rb.linearVelocity += transform.up * (approachSpeed - maxGroundApproachSpeed);
        }

        if (isGrounded)
        {
            float intoGroundSpeed = Vector3.Dot(rb.linearVelocity, -transform.up);
            if (intoGroundSpeed > 0f)
            {
                rb.linearVelocity += transform.up * intoGroundSpeed;
            }
        }

        if (isGrounded && groundedState != null && stateMachine != null)
        {
            jumpHoldActive = false;
            jumpHoldTimer = 0f;
            stateMachine.ChangeState(groundedState);
        }
    }

    public void BeginJumpHold()
    {
        jumpHoldActive = true;
        jumpHoldTimer = Mathf.Max(0f, jumpHoldMaxTime);
    }

    public void PreserveGrappleMomentum(Vector3 velocity, float preserveTime)
    {
        float speed = velocity.magnitude;
        if (speed <= maxAirSpeed)
        {
            return;
        }

        preservedGrappleSpeed = Mathf.Max(preservedGrappleSpeed, speed);
        preservedGrappleVelocity = velocity;
        grappleSpeedPreserveTimer = Mathf.Max(0f, preserveTime);
    }

    public void RefreshSensors()
    {
        Vector3 origin = transform.position + transform.up * 0.2f;
        Vector3 direction = -transform.up;
        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        isGrounded = false;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider != null && hitCollider.transform.root == transform.root)
            {
                continue;
            }

            isGrounded = true;
            Debug.Log("Ground ray hit collider: " + hitCollider.name);
            break;
        }

        Debug.DrawRay(origin, direction * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void OnTriggerEnter(Collider other)
    {
        CacheAsteroid(other);
    }

    void OnTriggerStay(Collider other)
    {
        CacheAsteroid(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (currentAsteroid != null && other.gameObject == currentAsteroid.gameObject)
        {
            currentAsteroid = null;
        }
    }

    void CacheAsteroid(Collider other)
    {
        if (!other.CompareTag("asteroid"))
        {
            return;
        }

        Asteroid candidate = other.GetComponent<Asteroid>();
        if (candidate == null)
        {
            return;
        }

        if (currentAsteroid == null)
        {
            currentAsteroid = candidate;
            return;
        }

        float candidateDistance = (candidate.transform.position - transform.position).sqrMagnitude;
        float currentDistance = (currentAsteroid.transform.position - transform.position).sqrMagnitude;
        if (candidateDistance < currentDistance)
        {
            if (candidate != currentAsteroid)
            {
                asteroidSwitchSlowTimer = asteroidSwitchSlowDuration;
            }
            currentAsteroid = candidate;
        }
    }
}

