using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation : MonoBehaviour
{
    [SerializeField] private bool playing = false;
    [SerializeField] private float speed = 1;
    [SerializeField] private Movement[] Parts = new Movement[0];
    private float lastStarted = 0;
    public bool is_running(){ return playing; }
    public void play()
    { 
        playing = true;
        lastStarted = Time.time;
        foreach (Movement P in Parts) P.run(speed);
    }
    public void stop()
    { 
        playing = false;
        foreach (Movement P in Parts) {P.stop(); P.setZero();}
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) play();
        if (!playing) return;
    }

}

