using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private bool running = false;
    [SerializeField] AnimationCurve[] PositionOverTime = new AnimationCurve[3]{null, null, null};
    private Vector3 zeroPos = Vector3.zero;
    [SerializeField] AnimationCurve[] ScaleOverTime = new AnimationCurve[3]{null, null, null};
    private Vector3 zeroScale = Vector3.zero;
    [SerializeField] AnimationCurve[] RotationOverTime = new AnimationCurve[3]{null, null, null};
    private Vector3 zeroRot = Vector3.zero;

    private void Start()
    {
        zeroPos = transform.localPosition;
        zeroScale = transform.localScale;
        zeroRot = transform.localRotation.eulerAngles;
    }
    private float lastStarted = 0;
    public bool is_running(){ return running; }
    private float time_modifier = 1;
    public void run(float timeMod)
    { 
        running = true;
        time_modifier = timeMod;
        lastStarted = Time.time;
    }
    public void stop()
    { 
        running = false;
        setZero();
    }
    public void setZero()
    {
        transform.localPosition = zeroPos;
        transform.localScale = zeroScale;
        transform.localRotation = Quaternion.Euler(zeroRot.x, zeroRot.y, zeroRot.z);
    }

    private void Update()
    {
        if (!running) return;
        float t = (Time.time-lastStarted) * time_modifier;
        transform.localPosition = zeroPos + new Vector3(PositionOverTime[0].Evaluate(t), PositionOverTime[1].Evaluate(t), PositionOverTime[2].Evaluate(t));
        transform.localScale = zeroScale + new Vector3(ScaleOverTime[0].Evaluate(t), ScaleOverTime[1].Evaluate(t), ScaleOverTime[2].Evaluate(t));
        transform.localRotation = Quaternion.Euler(zeroRot.x + RotationOverTime[0].Evaluate(t), zeroRot.y + RotationOverTime[1].Evaluate(t), zeroRot.z + RotationOverTime[2].Evaluate(t));
    }
}


