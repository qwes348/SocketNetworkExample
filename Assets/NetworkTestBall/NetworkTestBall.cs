using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTestBall : MonoBehaviour
{
    public float spawnedTime;
    private TransformSync myTrSync;

    private void Awake()
    {
        spawnedTime = Time.time;

        myTrSync = GetComponent<TransformSync>();        
    }

    private void Update()
    {
        //myTrSync.syncInterval = TestBallSpawner.instance.syncInterval;
        if (!myTrSync.isAvatar)
            Move();
    }

    private void Move()
    {
        transform.position = Vector3.up * Mathf.Sin(Mathf.PI * Time.time - spawnedTime) + Vector3.right * Mathf.Sin(Mathf.PI * (Time.time - spawnedTime) * 2f);
    }
}
