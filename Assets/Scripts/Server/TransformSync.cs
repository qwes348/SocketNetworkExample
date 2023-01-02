using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Jamong.Server;

[RequireComponent(typeof(JamongView))]
public class TransformSync : MonoBehaviour
{
    public int id;
    public bool isAvatar;
    public bool syncVelocity;
    public Rigidbody rb;
    public float syncMoveSpeed = 20f;
    public float syncInterval = 0.03f;
    public bool isTest;

    private Vector3 receivePos;
    private Quaternion receiveRot;
    private Vector3 receiveVelocity;

    private Coroutine runningSyncCor;
    private JamongView jv;
    private Vector3 lastSyncPos;
    private float moveDistanceFromLastSyncPos = 100f;

    #region 프로퍼티
    public JamongView MyJamongView
    {
        get
        {
            if (jv == null)
                jv = GetComponent<JamongView>();
            return jv;
        }
    }
    public Vector3 LastSyncPos => lastSyncPos;
    public float MoveDistance => moveDistanceFromLastSyncPos;
    #endregion

    public bool IsMine
    {
        get
        {
            if (jv == null)
                jv = GetComponent<JamongView>();
            if (!jv.IsInitComplete)
                return false;

            return jv.IsMine;
        }
    }

    private void Awake()
    {
        if (jv == null)
            jv = GetComponent<JamongView>();

        if (rb == null && syncVelocity)
            rb = GetComponent<Rigidbody>();
        if (rb == null && syncVelocity)
            enabled = false;

        //AIG_GameServer.Instance.onTransformSyncAck += OnTransformSyncReceive;
    }

    private void OnEnable()
    {
        runningSyncCor = StartCoroutine(SyncCor());
    }

    private void Start()
    {
        if (SyncObjectContainer.instance != null)
        {
            // 모든 싱크 오브젝트
            SyncObjectContainer.instance.allTransformSyncsList.Add(this);

            if (IsMine)
            {
                // 내 싱크 오브젝트
                SyncObjectContainer.instance.allMineTransformSyncList.Add(this);
            }
            else
            {
                // 타 플레이어의 싱크 오브젝트
                SyncObjectContainer.instance.allOtherTransformSyncDict.Add(MyJamongView.ViewID, this);
            }
        }
    }

    private IEnumerator SyncCor()
    {
        while (true)
        {
            if (isAvatar)
            {
                if (Vector3.Distance(transform.position, receivePos) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, receivePos, Time.deltaTime * syncMoveSpeed);
                    //transform.position = receivePos;
                }

                if (transform.rotation != receiveRot)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, receiveRot, Time.deltaTime * syncMoveSpeed);
                    //transform.rotation = receiveRot;
                }
                yield return null;
            }
            else
                yield return null;

            // 트랜스폼 싱크 보내는부분 SyncObjectContainer클래스로 옮김
            //else
            //{
            //    if (rb != null && syncVelocity)
            //        AIG_GameServer.Instance.proxy.TransformSyncReq(0, id, transform.position, rb.velocity, transform.rotation, JsonMessage.TargetEnum.OtherPlayers);
            //    else
            //        AIG_GameServer.Instance.proxy.TransformSyncReq(0, id, transform.position, Vector3.zero, transform.rotation, JsonMessage.TargetEnum.OtherPlayers);

            //    yield return new WaitForSeconds(syncInterval);
            //}            
        }
    }

    public void OnTransformSyncReceive(int objectID, Vector3 pos, Vector3 velocity, Quaternion rot)
    {
        if (MyJamongView.ViewID != objectID)
            return;
        if (!gameObject.activeSelf)
            return;

        receivePos = pos;
        receiveRot = rot;

        if (rb != null && syncVelocity)
        {
            rb.velocity = velocity;
            receiveVelocity = velocity;
        }
    }

    public void SetLastSyncPos(Vector3 pos)
    {
        lastSyncPos = pos;
        moveDistanceFromLastSyncPos = 0f;
    }
}
