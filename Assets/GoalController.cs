using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public static EventHandler OnReachGoalEvent;

    public void InvokeGoalReached()
    {
        OnReachGoalEvent?.Invoke(this, null);
    }
}
