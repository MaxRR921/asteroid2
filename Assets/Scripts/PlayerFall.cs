using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFall : MonoBehaviour
{

    public Asteroid asteroid;
    public Transform feet;
    public bool hasFlipped = false;
    public bool flipping = false;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (flipping)
        {
            asteroid.Flip(this.transform);
        }
        if (asteroid != null)
        {
            if (!flipping)
            {
                asteroid.Fall(this.transform);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("asteroid"))
        {
            StartCoroutine("Wait");
            asteroid = other.gameObject.GetComponent<Asteroid>();
        }
    }


    IEnumerator Wait()
    {
        flipping = true;
        yield return new WaitForSeconds(2f);
        flipping = false;
    }

}


