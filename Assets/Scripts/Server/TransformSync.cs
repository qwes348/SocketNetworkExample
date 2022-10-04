using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIGears.Server;

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

    public bool IsMine { get => !isAvatar; }

    private void Awake()
    {
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
            // ��� ��ũ ������Ʈ
            SyncObjectContainer.instance.allTransformSyncsList.Add(this);

            if(IsMine)
            {
                // �� ��ũ ������Ʈ
                SyncObjectContainer.instance.allMineTransformSyncList.Add(this);
            }
            else
            {
                // Ÿ �÷��̾��� ��ũ ������Ʈ
                SyncObjectContainer.instance.allOtherTransformSyncDict.Add(id, this);
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

            // Ʈ������ ��ũ �����ºκ� SyncObjectContainerŬ������ �ű�
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
        if (id != objectID)
            return;
        if (!gameObject.activeSelf)
            return;

        receivePos = pos;
        receiveRot = rot;        

        if(rb != null && syncVelocity)
            rb.velocity = velocity;
    }
}
