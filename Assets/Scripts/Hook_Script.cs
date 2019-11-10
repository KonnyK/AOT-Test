using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook_Script : MonoBehaviour
{
    [SerializeField] private bool debug = true;
    public enum HookStatus{shooting_hook, ready, cooldown, holding_player};
    [SerializeField] private GameObject HookPrefab;
    [SerializeField] private float ShootForce = 1000;
    [SerializeField] private float ReelForce = 100;
    [SerializeField] private float GasForce = 10;
    private Transform Hook;
    private LineRenderer Rope = null; 
    [SerializeField] public float MaxRopeLength = 250;
    private Rigidbody PlayerRB = null;
    private HookStatus currentStatus = HookStatus.cooldown;
    private bool ReelCooldownActive = false;
    private void ActivateReelCooldown()
    { 
        if (ReelCooldownActive) return;
        ReelCooldownActive = true;
        Invoke("reelCooldown", 1);
    }
    public void reelCooldown() { ReelCooldownActive = false; }

    private bool ReadyCooldownActive = false;
    private void ActivateReadyCooldown()
    {
        if (ReadyCooldownActive) return;
        ReadyCooldownActive = true;
        Invoke("readyCooldown", 0.5f);
    }
    public void readyCooldown(){ ReadyCooldownActive = false; }

    private bool retracted, inUse;
    /* Explanation:
                    0           0       shooting hook
                    0           1       holding player
                    1           0       ready
                    1           1       cooldown
     */

    private void setHookStatus(HookStatus newStatus)
    {
        currentStatus = newStatus;
        switch (newStatus)
        {
            case HookStatus.shooting_hook: {   retracted = false;     inUse = false;    break; }
            case HookStatus.holding_player: {  retracted = false;     inUse = true;     break; }
            case HookStatus.ready: {           retracted = true;      inUse = false;    break; }
            case HookStatus.cooldown: {        retracted = true;      inUse = true;     break; }
            default: { Debug.Log("Wrong Status assigned!", this); retracted=inUse=true; break; }
        }
    }
    private void Start()
    {
        Rope = GetComponent<LineRenderer>();
        Hook = Instantiate(HookPrefab, transform.position, Quaternion.identity, transform.parent.parent).transform;
        PlayerRB = transform.parent.GetComponent<Rigidbody>();
        resetHook();
    }
    
    public void Update()
    {
        if (debug) Debug.Log(currentStatus);
        if (Rope==null || Hook == null || HookPrefab==null || PlayerRB == null) return;

        if (!retracted) 
        {
            Rope.SetPositions(new Vector3[]{transform.position, Hook.transform.position});
            if (inUse)
            {
                Vector3 RopeDir = Hook.position - transform.position;
                //if (pullOnly) PlayerRB.AddForce(PullForce * RopeDir.normalized, ForceMode.Impulse);
                if ((transform.position + PlayerRB.velocity * Time.deltaTime - Hook.position).magnitude > RopeDir.magnitude)
                {  
                    //PlayerRB.AddForce(GasForce * Vector3.ProjectOnPlane(PlayerRB.velocity, RopeDir).normalized, ForceMode.Impulse);    
                    PlayerRB.AddForce(GasForce * (Vector3.ProjectOnPlane(PlayerRB.velocity, RopeDir) - PlayerRB.velocity).normalized, ForceMode.Impulse);    
                    Debug.DrawRay(transform.position, PlayerRB.velocity.normalized * 5, Color.red, 10);
                    Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(PlayerRB.velocity, RopeDir).normalized * 5, Color.green, 10);
                    Debug.DrawRay(transform.position, (Vector3.ProjectOnPlane(PlayerRB.velocity, RopeDir) - PlayerRB.velocity).normalized * 5, Color.blue, 10);
                    Debug.DrawRay(transform.position, RopeDir.normalized, Color.black, 10);
                }
            }
        }
    }

    public void resetHook()
    {
        if (ReadyCooldownActive || retracted) return;
        setHookStatus(HookStatus.cooldown);
        Hook.position = transform.position;
        foreach (Rigidbody RB in Hook.GetComponentsInChildren<Rigidbody>()) { RB.velocity = Vector3.zero; RB.useGravity = false; RB.constraints = RigidbodyConstraints.None;}
        PlayerRB.drag = 0;
        Hook.gameObject.SetActive(false);
        Rope.enabled = false;
        setHookStatus(HookStatus.ready);
        ActivateReadyCooldown();
    }
    public void reel(float Sign)
    {
        if (retracted || !inUse || ReelCooldownActive) return;
        PlayerRB.AddForce(Sign * Mathf.Max(ReelForce * PlayerRB.velocity.magnitude, 500) * (Hook.position - transform.position).normalized, ForceMode.Impulse);
        ActivateReelCooldown();
    }
    public void OnHookHit()
    {
        setHookStatus(HookStatus.holding_player);
    }

    public void ShootHook(Vector3 Direction)
    {
        if (currentStatus != HookStatus.ready) return;
        Vector3 Dir = Direction.normalized;
        Hook.gameObject.SetActive(true);
        Rope.enabled = true;
        Hook.transform.position = transform.position;
        Hook.rotation = Quaternion.LookRotation(Dir);
        Hook.GetComponent<Rigidbody>().AddForce(Dir * ShootForce, ForceMode.VelocityChange);
        Hook.GetComponent<HookTrigger>().setParent(this);
        setHookStatus(HookStatus.shooting_hook);
        ActivateReadyCooldown();
    }
    public void useGas(bool ON)
    {
        if (retracted || !inUse) return;
        if (ON) 
        {
            PlayerRB.drag = 0;
        }
        else 
        {
            PlayerRB.drag = 1;
        }
    }

}
