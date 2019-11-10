using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem Gas = null;
    [SerializeField] ParticleSystem Burst = null;
    [SerializeField] ParticleSystem Blood = null;
    
    public void playGas(bool Status)
    { 
        if (Gas == null) return;
        var G = Gas.emission;
        if (Status) G.rateOverTimeMultiplier = 10;
        else G.rateOverTimeMultiplier  = 0;
    }

    public void playBurst(Vector3 Dir)
    {
        if (Burst == null) return;
        Burst.transform.rotation = Quaternion.LookRotation(Dir);
        Burst.Play();
    }

}
