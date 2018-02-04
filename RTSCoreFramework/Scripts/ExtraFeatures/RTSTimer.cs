using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSTimer {

    float timerRate = 0.0f;
    float currentTimer = 0.0f;

    public RTSTimer(float rate)
    {
        timerRate = rate;
    }

    public void StartTimer()
    {
        currentTimer = Time.time + timerRate;
    }

    public bool IsTimerFinished()
    {
        return Time.time > currentTimer;
    }
}
