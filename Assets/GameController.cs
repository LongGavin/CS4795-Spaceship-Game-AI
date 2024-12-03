using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GoalController;
using static AISpaceshipController;
using TMPro;

public class GameController : MonoBehaviour
{
    public float roundLength = 300;
    public int shipCount = 1;
    public int shipsCompleted;

    public TextMeshProUGUI timerText;

    bool roundActive = false;

    public static EventHandler OnResetLevelEvent;

    float timer;

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
            Debug.Log("Invoke OnResetLevelEvent Due to All Ships Completed");
            OnResetLevelEvent?.Invoke(this, null);
            StartCoroutine(RoundTimer());
        }
        else if (!roundActive)
        {
            shipsCompleted = 0;
            Debug.Log("Invoke OnResetLevelEvent Due to Timer Run Out");
            OnResetLevelEvent?.Invoke(this, null);
            StartCoroutine(RoundTimer());
        }

    }

    private void FixedUpdate()
    {
        timerText.text = "" + timer;
    }

    IEnumerator RoundTimer()
    {
        roundActive = true;
        timer = roundLength;
        while (timer >= 0)
        {
            yield return new WaitForSeconds(1);
            timer--;
        }
        
        
        roundActive = false;
    }

    void IncrementCompletionCounter(object sender, EventArgs args)
    {
        shipsCompleted++;
    }
}
