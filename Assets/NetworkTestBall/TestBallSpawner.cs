using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIGears.Server;

public class TestBallSpawner : MonoBehaviour
{
    public static TestBallSpawner instance;

    public NetworkTestBall prefab;
    public float syncInterval;
    [SerializeField]
    private int id = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        /*
         * 2022-07-06
         * 현재 오브젝트 소환 알림이 서버에는 도착하지만
         * 서버에서 다른 플레이어에게 간헐적으로 전달해주지 않는 버그가 있음
         * 아직 고치지 못한상태로 프로젝트 중단
         */
        if (Input.GetMouseButtonDown(1))
        {
            var trSync = Instantiate(prefab, transform).GetComponent<TransformSync>();
            trSync.id = id;
            AIG_GameServer.Instance.proxy.PoolableSpawnSyncReq(0, id, prefab.GetComponent<Poolable>().id, JsonMessage.TargetEnum.OtherPlayers);
            id++;
        }
    }

    public void ChangeSyncInterval(string value)
    {
        if (float.TryParse(value, out float newInterval))
            syncInterval = newInterval;

        SyncObjectContainer.instance.SetSyncInterval(syncInterval);
    }
}
