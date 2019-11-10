using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private PlayerParticleManager Particles;
    [SerializeField] private Transform PlayerObject = null;
    [SerializeField] private Hook_Script[] Hooks = new Hook_Script[2];
    [SerializeField] private Transform OrientationReference = null; //the Camera Transform
    private Rigidbody PlayerRB = null;


    [SerializeField] private KeyCode[] WASD_Controls = new KeyCode[4]{KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D};
    [SerializeField] float WalkSpeed = 3;
    [SerializeField, Range(0,1)] private float BrakeSpeed = 0.9f;
    [SerializeField, Range(0, 1)] private float Smoothness = 0.9f;
    public Vector3 NextMovingDir2D = Vector3.zero;

    [SerializeField] private KeyCode[] HookKeys = new KeyCode[2]{KeyCode.E, KeyCode.Q};
    [SerializeField] private KeyCode GasKey = KeyCode.LeftShift;
    [SerializeField] float GasSpeed = 3;
    [SerializeField] float BurstForce = 300;
    private float[] LastGasTime = new float[]{0, 0, 0, 0};
    private float LastBurstTime = 0;
    private float BurstActivationTime = 0.2f;
    private float BurstCooldown = 1;

    [SerializeField] private KeyCode Jump = KeyCode.Space;
    [SerializeField,Range(1, 5000)] private float Jumpforce = 500;
    private float LastJumpTime = 0;
    private float JumpCooldown = 1;
    private bool OnGround = true;
    private ContactPoint[] LastContacts = new ContactPoint[]{};

    
    public void Start()
    {
        PlayerRB = PlayerObject.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        if (PlayerObject == null || OrientationReference == null) return;

        Vector3 Forward = Vector3.ProjectOnPlane(OrientationReference.forward, Vector3.up);
        Vector3 Right = Vector3.ProjectOnPlane(OrientationReference.right, Vector3.up);
        Vector3 WantedDir = Vector3.zero;

        //registering input for walking, gas and burst
        for (int i = 0; i < WASD_Controls.Length; i++) 
        {
            Vector3 InputVector = new Vector3( (i-2)%2 , 0 , -(i-1)%2 );
            if (Input.GetKeyDown(WASD_Controls[i]) && !OnGround && Input.GetKey(GasKey) && Time.time-LastGasTime[i] < BurstActivationTime && Time.time-LastBurstTime > BurstCooldown)
            {//burst
                Debug.Log("Burst " + i + "activated.");
                PlayerRB.AddForce(BurstForce * (Forward * InputVector.z + Right * InputVector.x), ForceMode.Impulse);
                Particles.playBurst(-(Forward * InputVector.z + Right * InputVector.x).normalized);
                LastBurstTime = Time.time;   
            } 
            if (Input.GetKey(WASD_Controls[i])) WantedDir += InputVector;
            if (Input.GetKeyUp(WASD_Controls[i])) LastGasTime[i] = Time.time;
        }
        if (WantedDir == Vector3.zero) NextMovingDir2D *= 1 - BrakeSpeed;
        else NextMovingDir2D = Vector3.Lerp(NextMovingDir2D, WantedDir.normalized, (1 - Smoothness)).normalized;

        //executing walking/gas
        Vector3 MoveDir = (Forward * NextMovingDir2D.z + Right * NextMovingDir2D.x);
        if (OnGround) PlayerRB.AddForce(WalkSpeed * MoveDir, ForceMode.Impulse);
        else if (Input.GetKey(GasKey)) PlayerRB.AddForce(GasSpeed * MoveDir, ForceMode.Impulse);

        //Hooks
        for(int i = 0; i < Hooks.Length; i++)
        {
            if (Input.GetKey(HookKeys[i])) Hooks[i].ShootHook(OrientationReference.forward);
            else Hooks[i].resetHook();
            if (Input.mouseScrollDelta.y != 0) Hooks[i].reel(-Input.mouseScrollDelta.y);
            if (Input.GetKey(GasKey)) Hooks[i].useGas(true);
            else Hooks[i].useGas(false);
        }

        //Jumping
        if (Input.GetKey(Jump) && OnGround && (Time.time-LastJumpTime) > JumpCooldown)
        {
            PlayerRB.AddForce(Vector3.up * Jumpforce, ForceMode.Impulse);
            LastJumpTime = Time.time;
        }
        if (NextMovingDir2D.magnitude == 1) PlayerObject.rotation = Quaternion.LookRotation(MoveDir, Vector3.up);

        Particles.playGas(Input.GetKey(GasKey));
    }

    public void OnCollisionStay(Collision other)
    { 
        OnGround = true; 
        other.GetContacts(LastContacts);
    }
    public void OnCollisionExit(Collision other){ OnGround = false; }

}

