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
    public float airControlAcceleration = 20f;
    public float maxAirSpeed = 40f;
    public float groundCheckDistance = 5f;
    public LayerMask groundMask = Physics.DefaultRaycastLayers;

    Asteroid currentAsteroid;
    Vector3 airInput;
    bool isGrounded;

    public Asteroid CurrentAsteroid
    {
        get { return currentAsteroid; }
    }

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

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

        if (playerLook == null)
        {
            playerLook = GetComponent<PlayerLook>();
        }
    }

    public override void stateUpdate()
    {
        airInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
    }

    public override void stateFixedUpdate()
    {
        RefreshSensors();

        if (currentAsteroid == null)
        {
            return;
        }

        Quaternion targetUpRotation = currentAsteroid.GetTargetUpRotation(rb.rotation, rb.position);
        Quaternion alignedRotation = Quaternion.Slerp(rb.rotation, targetUpRotation, alignSpeed * Time.fixedDeltaTime);
        float yawDelta = playerLook != null ? playerLook.ConsumeYawDelta() : 0f;
        if (Mathf.Abs(yawDelta) > 0f)
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, alignedRotation * Vector3.up);
            alignedRotation = yawRotation * alignedRotation;
        }
        rb.MoveRotation(alignedRotation);
        Vector3 gravityAcceleration = currentAsteroid.GetGravityAcceleration(rb.position);
        rb.AddForce(gravityAcceleration, ForceMode.Acceleration);

        Vector3 desiredWorld = transform.TransformDirection(airInput);
        Vector3 tangentInput = Vector3.ProjectOnPlane(desiredWorld, transform.up).normalized;
        rb.AddForce(tangentInput * airControlAcceleration, ForceMode.Acceleration);

        Vector3 upVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        Vector3 tangentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, transform.up);
        if (tangentVelocity.magnitude > maxAirSpeed)
        {
            tangentVelocity = tangentVelocity.normalized * maxAirSpeed;
            rb.linearVelocity = tangentVelocity + upVelocity;
        }

        if (isGrounded && groundedState != null && stateMachine != null)
        {
            stateMachine.ChangeState(groundedState);
        }
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
            currentAsteroid = candidate;
        }
    }
}
