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
         * ���� ������Ʈ ��ȯ �˸��� �������� ����������
         * �������� �ٸ� �÷��̾�� ���������� ���������� �ʴ� ���װ� ����
         * ���� ��ġ�� ���ѻ��·� ������Ʈ �ߴ�
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
