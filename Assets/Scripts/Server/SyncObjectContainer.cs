using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIGears.Server;

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

    private void Awake()
    {
        if(instance == null)
            instance = this;

        AIG_GameServer.Instance.onMultipleTransformReceiveAck += OnTransformSyncReceive;
    }

    private void Start()
    {
        if (runningSyncCor == null)
            runningSyncCor = StartCoroutine(TransformSyncCor());
    }

    public TransformSync FindTransformSync(int id, bool isMine)
    {
        return allTransformSyncsList.Find(t => t.id == id && t.IsMine == isMine);
    }

    public int GetNewSyncID()
    {
        allTransformSyncsList.Sort((TransformSync a, TransformSync b) => a.id.CompareTo(b.id));
        return allTransformSyncsList[allTransformSyncsList.Count - 1].id + 1;
    }

    public void SetSyncInterval(float value)
    {
        syncInterval = value;
    }

    IEnumerator TransformSyncCor()
    {
        while(true)
        {
            //AIG_GameServer.Instance.proxy.MultipleTransformSyncReq(0, allMineTransformSyncList);
            if(allMineTransformSyncList.Count > 0)
                AIG_GameServer.Instance.proxy.MultipleTransformSyncReq(0, allMineTransformSyncList, JsonMessage.TargetEnum.OtherPlayers);

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
