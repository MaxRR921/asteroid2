using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    public Transform cam;
    public Transform projectileSpawnPoint;
    public GameObject projectilePrefab;
    public LayerMask aimLayers = Physics.DefaultRaycastLayers;

    public float projectileSpeed = 40f;
    public float projectileLifetime = 5f;
    public float maxAimDistance = 200f;
    public float shotCooldown = 0.15f;

    private Camera playerCamera;
    private float shotCooldownTimer;
    private Collider[] ownerColliders;

    void Start()
    {
        if (cam != null)
        {
            playerCamera = cam.GetComponent<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        ownerColliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        if (shotCooldownTimer > 0f)
        {
            shotCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || shotCooldownTimer > 0f)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        Vector3 targetPoint = GetTargetPoint();
        Vector3 shotDirection = (targetPoint - spawnPosition).normalized;

        if (shotDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shotDirection));
        IgnoreOwnerCollision(projectile);

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb == null)
        {
            projectileRb = projectile.GetComponentInChildren<Rigidbody>();
        }

        if (projectileRb != null)
        {
            projectileRb.isKinematic = false;
            projectileRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            projectileRb.linearVelocity = Vector3.zero;
            projectileRb.angularVelocity = Vector3.zero;
            projectileRb.AddForce(shotDirection * projectileSpeed, ForceMode.VelocityChange);
        }

        Destroy(projectile, projectileLifetime);
        shotCooldownTimer = shotCooldown;
    }

    private Vector3 GetSpawnPosition()
    {
        if (projectileSpawnPoint != null)
        {
            return projectileSpawnPoint.position;
        }

        return transform.position;
    }

    private Vector3 GetTargetPoint()
    {
        Ray aimRay = GetAimRay();
        if (Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance, aimLayers))
        {
            return hit.point;
        }

        return aimRay.origin + aimRay.direction * maxAimDistance;
    }

    private Ray GetAimRay()
    {
        if (playerCamera != null)
        {
            return playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        if (cam != null)
        {
            return new Ray(cam.position, cam.forward);
        }

        return new Ray(transform.position, transform.forward);
    }

    private void IgnoreOwnerCollision(GameObject projectile)
    {
        if (ownerColliders == null || ownerColliders.Length == 0)
        {
            return;
        }

        Collider[] projectileColliders = projectile.GetComponentsInChildren<Collider>();
        if (projectileColliders == null || projectileColliders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < ownerColliders.Length; i++)
        {
            Collider ownerCollider = ownerColliders[i];
            if (ownerCollider == null)
            {
                continue;
            }

            for (int j = 0; j < projectileColliders.Length; j++)
            {
                Collider projectileCollider = projectileColliders[j];
                if (projectileCollider == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(ownerCollider, projectileCollider, true);
            }
        }
    }
}
