using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState currentState;
    public GameObject player;

    public void Start()
    {
        //currentState = player.GetComponent<TestPickup>();
        currentState = player.GetComponent<PlayerController>();
        currentState.stateStart();
    }

    public void Update()
    {
        currentState.stateUpdate();
    }


    public void exitState()
    {

    }
}
