using Unity.VisualScripting;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerController playerController;
    public Transform cam;
    public LayerMask whatIsGrappleable = 3;
    public LineRenderer lr;


    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float grapplePullAcceleration = 45f;
    public float grappleTensionAcceleration = 90f;
    public float ropeTightenSpeed = 18f;
    public float minRopeLength = 4f;
    public float maxRadialSpeed = 40f;
    public float maxPullSpeed = 35f;
    public float grappleSpeedPreserveTime = 0.9f;
    public float maxFovIncrease = 12f;
    public float sprintFovIncrease = 6f;
    public float fovLerpSpeed = 8f;

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
    private Camera grappleCamera;
    private float baseFov;
    private float currentRopeLength;

    private Vector3 GetGrappleOrigin()
    {
        return transform.position;
    }

    private Ray GetAimRay()
    {
        if (grappleCamera != null)
        {
            return grappleCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        if (cam != null)
        {
            return new Ray(cam.position, cam.forward);
        }

        return new Ray(transform.position, transform.forward);
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        if (cam != null)
        {
            grappleCamera = cam.GetComponent<Camera>();
        }
        if (grappleCamera == null)
        {
            grappleCamera = Camera.main;
        }
        if (grappleCamera != null)
        {
            baseFov = grappleCamera.fieldOfView;
        }
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

        UpdateFov();
    }

    private void StartGrapple()
    {

        if (grapplingCooldownTimer > 0f)
        {
            return;
        }

        grappling = true;

        RaycastHit hit;
        Ray aimRay = GetAimRay();
        if (Physics.Raycast(aimRay, out hit, maxGrappleDistance, whatIsGrappleable))
        {

            grapplePoint = hit.point;
            hitGrappleable = true;
            pulling = false;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = aimRay.origin + aimRay.direction * maxGrappleDistance;
            hitGrappleable = false;
            pulling = false;

        }
        lr.enabled = true;
        lr.SetPosition(0, GetGrappleOrigin());
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            lr.SetPosition(0, GetGrappleOrigin());
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
        currentRopeLength = Vector3.Distance(rb.position, grapplePoint);
    }

    private void StopGrapple()
    {
        CancelInvoke(nameof(ExecuteGrapple));
        CancelInvoke(nameof(StopGrapple));

        if (pulling && airborneState != null && rb != null)
        {
            airborneState.PreserveGrappleMomentum(rb.linearVelocity, grappleSpeedPreserveTime);
        }

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
        float distanceToPoint = toPoint.magnitude;
        if (distanceToPoint <= 0.0001f)
        {
            return;
        }

        Vector3 pullDirection = toPoint / distanceToPoint;
        currentRopeLength = Mathf.Max(minRopeLength, currentRopeLength - (ropeTightenSpeed * Time.fixedDeltaTime));

        float ropeStretch = Mathf.Max(0f, distanceToPoint - currentRopeLength);
        float pullAcceleration = grapplePullAcceleration + (ropeStretch * grappleTensionAcceleration);
        rb.AddForce(pullDirection * pullAcceleration, ForceMode.Acceleration);

        float radialSpeed = Vector3.Dot(rb.linearVelocity, pullDirection);
        if (radialSpeed < -0.01f && ropeStretch > 0f)
        {
            rb.linearVelocity -= pullDirection * radialSpeed;
            radialSpeed = 0f;
        }

        if (radialSpeed > maxRadialSpeed)
        {
            rb.linearVelocity -= pullDirection * (radialSpeed - maxRadialSpeed);
        }
    }

    private void UpdateFov()
    {
        if (grappleCamera == null || rb == null)
        {
            return;
        }

        float targetFov = baseFov;
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool hasMoveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
        if (sprintHeld && hasMoveInput)
        {
            targetFov += sprintFovIncrease;
        }

        if (grappling && pulling)
        {
            float speedRatio = Mathf.Clamp01(rb.linearVelocity.magnitude / Mathf.Max(maxPullSpeed, 0.01f));
            targetFov += maxFovIncrease * speedRatio;
        }

        grappleCamera.fieldOfView = Mathf.Lerp(grappleCamera.fieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
    }


    // Update is called once per frame
}
