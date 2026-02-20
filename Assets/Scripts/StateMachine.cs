using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState currentState;
    public GameObject player;
    public BaseState startingState;

    void Awake()
    {
        if (player == null)
        {
            player = gameObject;
        }
    }

    void Start()
    {
        if (startingState != null)
        {
            ChangeState(startingState);
            return;
        }

        BaseState fallbackState = player.GetComponent<PlayerController>();
        if (fallbackState != null)
        {
            ChangeState(fallbackState);
        }
    }

    void Update()
    {
        Debug.Log("Current state: " + (currentState != null ? currentState.GetType().Name : "None"));
        if (currentState == null)
        {
            return;
        }
        if (currentState == null)
        {
            return;
        }

        currentState.stateUpdate();
    }

    void FixedUpdate()
    {
        if (currentState == null)
        {
            return;
        }
        Debug.Log(currentState.GetType().Name);
        Debug.Log("IS GROUNDED: " + player.GetComponent<PlayerFall>().IsGrounded);

        currentState.stateFixedUpdate();
    }

    public void ChangeState(BaseState nextState)
    {
        if (nextState == null || nextState == currentState)
        {
            return;
        }

        if (currentState != null)
        {
            currentState.stateExit();
        }

        currentState = nextState;
        currentState.stateStart();
    }
}
