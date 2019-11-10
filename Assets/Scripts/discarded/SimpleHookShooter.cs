using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHookShooter : MonoBehaviour
{
    public enum HookStatus{shooting_hook, pulling_hook, holding_player, pulling_player, ready_for_shot, error, retracting_complete}
    [SerializeField] private GameObject HookPrefab;
    [SerializeField] private float ShootForce = 1000;
    public float MaxRopeLength = 500;
    private float RopeLenth = 0;
    private Transform Hook;
    private LineRenderer Rope = null; 
    private Rigidbody PlayerRB = null;
    private HookStatus currentStatus = HookStatus.error;
    private bool ResetCooldownActive = false;
    private void ActivateResetCooldown()
    {
         ResetCooldownActive = true;
         Invoke("resetCooldown", 0.1f);
    }
    private bool retracting, retracted, inUse;
    /* Explanation:
                    0           0           0       shooting Hook
                    0           0           1       holding Player
                    0           1           0       ready for next shot
                    0           1           1       ERROR
                    1           0           0       pulling Hook
                    1           0           1       pulling Player
                    1           1           0       retracting completed
                    1           1           1       TRUE (=ERROR)
     */
     private void setHookStatus(HookStatus newStatus)
     {
         currentStatus = newStatus;
         switch (newStatus)
         {
             case HookStatus.shooting_hook:  {       retracting = false;     retracted = false;      inUse = false;    break; }
             case HookStatus.holding_player: {       retracting = false;     retracted = false;      inUse = true;     break; }
             case HookStatus.ready_for_shot: {       retracting = false;     retracted = true;       inUse = false;    break; }
             case HookStatus.error: {                retracting = false;     retracted = true;       inUse = true;     break; }
             case HookStatus.pulling_hook: {         retracting = true;      retracted = false;      inUse = false;    break; }
             case HookStatus.pulling_player: {       retracting = true;      retracted = false;      inUse = true;     break; }
             case HookStatus.retracting_complete: {  retracting = true;      retracted = true;       inUse = false;    break; }
             default: { Debug.Log("Wrong Status assigned!", this); retracting=retracted=inUse=true; break; }
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
        Debug.Log(currentStatus);
        if (Rope==null || Hook == null || HookPrefab==null || PlayerRB == null) return;

        if (!retracted) Rope.SetPositions(new Vector3[]{transform.position, Hook.transform.position});
        Vector3 nextPos = transform.position + PlayerRB.velocity * Time.deltaTime;
        if ((nextPos - Hook.position).magnitude > RopeLenth &&  (currentStatus == HookStatus.holding_player || currentStatus == HookStatus.pulling_player))  
        {
            //PlayerRB.velocity = (Hook.position + (nextPos - Hook.position).normalized * RopeLenth - transform.position)/Time.deltaTime;
            PlayerRB.velocity = PlayerRB.velocity.magnitude * Vector3.ProjectOnPlane(PlayerRB.velocity, (transform.position - Hook.position).normalized).normalized;
        }
/*
        if (currentStatus == HookStatus.holding_player && Vector3.Angle(PlayerRB.velocity.normalized, (transform.position - Hook.position).normalized) < 90)
        {
                PlayerRB.velocity = Vector3.ProjectOnPlane(PlayerRB.velocity, (transform.position - Hook.position).normalized);
        }
*/
        if (currentStatus == HookStatus.pulling_hook)
        {
            Hook.GetComponent<Rigidbody>().velocity = 0.5f * ShootForce * (transform.position-Hook.position);
            if ((Hook.position-transform.position).magnitude < 1) resetHook();
        }
    }
    public void resetHook()
    {
        if (ResetCooldownActive) return;
        setHookStatus(HookStatus.retracting_complete);
        Hook.position = transform.position;
        foreach (Rigidbody RB in Hook.GetComponentsInChildren<Rigidbody>()) { RB.velocity = Vector3.zero; RB.useGravity = true; }
        Hook.gameObject.SetActive(false); 
        Rope.enabled = false;
        RopeLenth = 0;
        Invoke("readyHook", 0.1f); 
        ActivateResetCooldown();
    }
    public void reel(float Force)
    {
        if (currentStatus != HookStatus.holding_player) return;
        setHookStatus(HookStatus.pulling_player);
        PlayerRB.AddForce(Mathf.Max(50 * PlayerRB.velocity.magnitude, ShootForce) * Force * (Hook.position - transform.position).normalized, ForceMode.Impulse);
        Invoke("reactivateReel", 0.05f);
    }
    private void reactivateReel()
    { 
        if (currentStatus != HookStatus.pulling_player) return;
        setHookStatus(HookStatus.holding_player); 
        RopeLenth = Mathf.Min(RopeLenth, (Hook.position-transform.position).magnitude);
    }
    public void OnHookHit()
    {
        setHookStatus(HookStatus.holding_player);
        RopeLenth = (Hook.position - transform.position).magnitude;
    }
    public void releaseHook()
    {
        if (currentStatus == HookStatus.holding_player || currentStatus == HookStatus.pulling_player || currentStatus == HookStatus.shooting_hook)
        setHookStatus(HookStatus.pulling_hook);
        foreach (Rigidbody RB in Hook.GetComponentsInChildren<Rigidbody>()) { RB.constraints = RigidbodyConstraints.None; RB.useGravity = true; }
    }
    public void ShootHook(Vector3 Direction)
    {
        Vector3 Dir = Direction.normalized;
        if (currentStatus != HookStatus.ready_for_shot) return;
        ActivateResetCooldown();
        Hook.gameObject.SetActive(true);
        Rope.enabled = true;
        RopeLenth = MaxRopeLength;
        Hook.transform.position = transform.position;
        Hook.rotation = Quaternion.LookRotation(Dir);
        Hook.GetComponent<Rigidbody>().AddForce(Dir * ShootForce, ForceMode.VelocityChange);
        //Hook.GetComponent<HookTrigger>().setParent(this);
        setHookStatus(HookStatus.shooting_hook);
    }
    public void readyHook(){ if (currentStatus == HookStatus.retracting_complete) setHookStatus(HookStatus.ready_for_shot); }
    public void resetCooldown(){ ResetCooldownActive = false; }

}
