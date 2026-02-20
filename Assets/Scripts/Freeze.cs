using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Freeze : BaseState 
{
    public StateMachine stateMachine;
    public PlayerController groundedState;

    public PlayerFall airborneState;

    public PlayerLook playerLook;   

    public Rigidbody rb;

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

        if (groundedState == null)
        {
            groundedState = GetComponent<PlayerController>();
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
        Debug.Log("Entered freeze state");
        rb.linearVelocity = Vector3.zero;
    }

    public override void stateUpdate()
    {

    }

    public override void stateFixedUpdate()
    {
        Debug.Log("In freeze state fixed update");
    }

}