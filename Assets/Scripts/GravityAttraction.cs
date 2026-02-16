using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAttraction : MonoBehaviour
{

    public float gravity = -5f;

    public void Attract(Transform body)
    {
        //first, orient body
        Vector3 targetDirection = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        body.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;

        //apply downward force (towards center of this object)
        body.GetComponent<Rigidbody>().AddForce(targetDirection * gravity);

    }
}
