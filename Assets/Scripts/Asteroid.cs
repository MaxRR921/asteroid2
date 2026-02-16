using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public float gravity = -2f;
    public Vector3 Position;
    public Transform feet;

    void Start()
    {
        Position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
    }




    // Update is called once per frame

    public void Flip(Transform body)
    {
        //first, orient body
        Vector3 targetDirection = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        //body.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;
        feet.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;

        body.rotation = Quaternion.Slerp(body.transform.rotation, feet.transform.rotation, 2f * Time.deltaTime);
        //apply downward force (towards center of this object)
        body.GetComponent<Rigidbody>().AddForce(targetDirection * gravity);
        Debug.Log("Flip");

    }

    public void Fall(Transform body)
    {
        //first, orient body
        Vector3 targetDirection = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        //body.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;
        feet.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;
        body.rotation = Quaternion.Slerp(body.transform.rotation, feet.transform.rotation, 10f * Time.deltaTime);
        //apply downward force (towards center of this object)
        body.GetComponent<Rigidbody>().AddForce(targetDirection * gravity);
        Debug.Log("Fall");

    }



}

