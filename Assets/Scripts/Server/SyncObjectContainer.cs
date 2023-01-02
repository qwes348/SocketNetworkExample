using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jamong.Server;
using System;

public class SyncObjectContainer : MonoBehaviour
{
    public static SyncObjectContainer instance;

    public List<TransformSync> allTransformSyncsList;
    // 내 객체들 모음 리스트
    public List<TransformSync> allMineTransformSyncList;
    // 타 플레이어의 객체들 모음 딕셔너리
    public Dictionary<int, TransformSync> allOtherTransformSyncDict = new Dictionary<int, TransformSync>();

    [SerializeField]
    private float syncInterval = 0.03f;

    private Coroutine runningSyncCor;

    private Queue<Action<int>> syncIdReceiveActionQueue;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        GameServer.Instance.onMultipleTransformReceiveAck += OnTransformSyncReceive;
        GameServer.Instance.onSyncIdReceiveAck += OnNewSyncIdReceive;

        syncIdReceiveActionQueue = new Queue<Action<int>>();
    }

    private void OnDestroy()
    {
        GameServer.Instance.onMultipleTransformReceiveAck -= OnTransformSyncReceive;
        GameServer.Instance.onSyncIdReceiveAck -= OnNewSyncIdReceive;
    }

    private void Start()
    {
        if (runningSyncCor == null)
            runningSyncCor = StartCoroutine(TransformSyncCor());
    }

    public TransformSync FindTransformSync(int id, bool isMine)
    {
        return allTransformSyncsList.Find(t => t.MyJamongView.ViewID == id && t.MyJamongView.IsMine == isMine);
    }

    public void GetNewSyncID(Action<int> onReceived)
    {
        //allTransformSyncsList.Sort((TransformSync a, TransformSync b) => a.id.CompareTo(b.id));
        //return allTransformSyncsList[allTransformSyncsList.Count - 1].id + 1;

        // 새 싱크ID 요청
        GameServer.Instance.proxy.NewSyncIdReq(0);
        // 서버에게 새ID 받았을때 실행할 액션을 큐에 넣어둠
        syncIdReceiveActionQueue.Enqueue(onReceived);
    }

    /// <summary>
    /// 서버에서 새 싱크ID를 받음
    /// </summary>
    /// <param name="newId"></param>
    private void OnNewSyncIdReceive(int newId)
    {
        if (syncIdReceiveActionQueue.Count <= 0)
            return;
        // 큐에서 액션을 하나 꺼내서 실행
        syncIdReceiveActionQueue.Dequeue()?.Invoke(newId);
    }

    public void SetSyncInterval(float value)
    {
        syncInterval = value;
    }

    IEnumerator TransformSyncCor()
    {
        while (true)
        {
            //AIG_GameServer.Instance.proxy.MultipleTransformSyncReq(0, allMineTransformSyncList);
            if (allMineTransformSyncList.Count > 0)
                GameServer.Instance.proxy.MultipleTransformSyncReq(0, allMineTransformSyncList, JsonMessage.TargetEnum.OtherPlayers);

            yield return new WaitForSeconds(syncInterval);
        }
    }

    private void OnTransformSyncReceive(List<TransformSyncReceived> syncList)
    {
        for (int i = 0; i < syncList.Count; i++)
        {
            var receivedTR = syncList[i];
            if (allOtherTransformSyncDict.ContainsKey(receivedTR.objectID))
            {
                var tr = allOtherTransformSyncDict[receivedTR.objectID];

                tr.OnTransformSyncReceive(receivedTR.objectID, receivedTR.position, receivedTR.syncVelocity ? receivedTR.velocity : Vector3.zero, receivedTR.rotation);
            }
        }
    }

    public void RemoveTransformSync(TransformSync trSync)
    {
        if (allTransformSyncsList.Contains(trSync))
            allTransformSyncsList.Remove(trSync);

        if (allMineTransformSyncList.Contains(trSync))
            allMineTransformSyncList.Remove(trSync);

        if (allOtherTransformSyncDict.ContainsKey(trSync.id) && allOtherTransformSyncDict[trSync.id] == trSync)
        {
            allOtherTransformSyncDict.Remove(trSync.id);
        }
    }
}

// 서버를 통해 받은 트랜스폼 싱크정보
public class TransformSyncReceived
{
    public int objectID;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public bool syncVelocity;

    public TransformSyncReceived()
    {

    }

    public TransformSyncReceived(int objectID, Vector3 position, Quaternion rotation, Vector3 velocity, bool syncVelocity)
    {
        this.objectID = objectID;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.syncVelocity = syncVelocity;
    }
}
