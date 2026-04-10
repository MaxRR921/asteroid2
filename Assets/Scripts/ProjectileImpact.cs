using UnityEngine;

public class ProjectileImpact : MonoBehaviour
{
    public float impactRadius = 1.2f;
    public float impactDepth = 0.45f;
    public float impactFalloff = 2.5f;
    public bool destroyOnImpact = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.contactCount == 0)
        {
            return;
        }

        Asteroid asteroid = collision.collider.GetComponentInParent<Asteroid>();
        if (asteroid != null)
        {
            ContactPoint contact = collision.GetContact(0);
            asteroid.DeformAtPoint(contact.point, contact.normal, impactRadius, impactDepth, impactFalloff);
        }

        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
}
