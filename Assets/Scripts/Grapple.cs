using Unity.VisualScripting;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerController playerController;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable = 3;
    public LineRenderer lr;


    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float grapplePullAcceleration = 45f;
    public float maxPullSpeed = 35f;

    private Vector3 grapplePoint;

    public float grappleingCooldown;
    private float grapplingCooldownTimer;

    public KeyCode grappleKey = KeyCode.Mouse1;

     public Freeze freezeState;

    public PlayerFall airborneState;
    private bool grappling;
    private bool pulling;
    private bool hitGrappleable;
    private Rigidbody rb;



    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }

        if (Input.GetKeyUp(grappleKey) && grappling)
        {
            StopGrapple();
        }

        if (grapplingCooldownTimer > 0f)
        {
            grapplingCooldownTimer -= Time.deltaTime;
        }
    }

    private void StartGrapple()
    {

        if (grapplingCooldownTimer > 0f)
        {
            return;
        }

        grappling = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {

            grapplePoint = hit.point;
            hitGrappleable = true;
            pulling = false;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            hitGrappleable = false;
            pulling = false;

        }
        lr.enabled = true;
        lr.SetPosition(0, gunTip.position);
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    private void ExecuteGrapple()
    {
        if (!grappling || !hitGrappleable)
        {
            return;
        }

        pulling = true;
    }

    private void StopGrapple()
    {
        CancelInvoke(nameof(ExecuteGrapple));
        CancelInvoke(nameof(StopGrapple));

        grappling = false;
        pulling = false;
        hitGrappleable = false;
        grapplingCooldownTimer = grappleingCooldown;
        lr.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!grappling || !pulling || rb == null)
        {
            return;
        }

        Vector3 toPoint = grapplePoint - rb.position;
        if (toPoint.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 pullDirection = toPoint.normalized;
        rb.AddForce(pullDirection * grapplePullAcceleration, ForceMode.Acceleration);

        Vector3 pullVelocity = Vector3.Project(rb.linearVelocity, pullDirection);
        Vector3 nonPullVelocity = rb.linearVelocity - pullVelocity;
        if (pullVelocity.magnitude > maxPullSpeed)
        {
            rb.linearVelocity = nonPullVelocity + pullDirection * maxPullSpeed;
        }
    }


    // Update is called once per frame
}
