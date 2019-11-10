using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private Vector3 SpawnPoint = Vector3.up * 10;
    [SerializeField] private float RespawnTime = 1;
    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private float Health = 1;

    public void Start()
    {
        Health = MaxHealth;
        transform.position = SpawnPoint;
    }

    private void Update()
    {
        if (Health <= 0) Invoke("Start", RespawnTime);   
    }
}