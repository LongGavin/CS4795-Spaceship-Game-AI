using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GoalController;
using static AISpaceshipController;

public class GameController : MonoBehaviour
{
    public float roundLength = 300;

    public int shipCount = 1;

    int shipsCompleted;

    bool roundActive = false;

    public static EventHandler OnResetLevelEvent;

    private void OnEnable()
    {
        OnReachGoalEvent += IncrementCompletionCounter;
        OnShipDestroyed += IncrementCompletionCounter;
    }

    private void OnDisable()
    {
        OnReachGoalEvent -= IncrementCompletionCounter;
        OnShipDestroyed -= IncrementCompletionCounter;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RoundTimer());
    }

    // Update is called once per frame
    void Update()
    {
        if (roundActive && shipsCompleted >= shipCount)
        {
            StopCoroutine(RoundTimer());
            roundActive = false;
            shipsCompleted = 0;
            OnResetLevelEvent?.Invoke(this, null);
            StartCoroutine(RoundTimer());
        }
        else if (!roundActive)
        {
            shipsCompleted = 0;
            OnResetLevelEvent?.Invoke(this, null);
            StartCoroutine(RoundTimer());
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            StopCoroutine(RoundTimer());
            roundActive = false;
            shipsCompleted = 0;
            OnResetLevelEvent?.Invoke(this, null);
            StartCoroutine(RoundTimer());
        }
    }

    IEnumerator RoundTimer()
    {
        roundActive = true;
        yield return new WaitForSeconds(roundLength);
        roundActive = false;
    }

    void IncrementCompletionCounter(object sender, EventArgs args)
    {
        shipsCompleted++;
    }
}
