using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    public virtual void stateStart()
    {
    }

    public virtual void stateUpdate()
    {
    }

    public virtual void stateFixedUpdate()
    {
    }

    public virtual void stateExit()
    {
    }
}
