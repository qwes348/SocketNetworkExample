using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGNet;
using System;
using NaughtyAttributes;

namespace AIGears.Server
{
    public class AIG_GameServer : Singleton2<AIG_GameServer>
    {
        public AIG_Client Client { get; private set; }

        public GameServer_proxy proxy = new GameServer_proxy();
        private GameServer_stub stub = new GameServer_stub();

        //public int port;

        public bool IsConnectServer { get => Client != null && Client.IsConnected; }

        public Action<AIG_Client> ActionOnConnected;

        public DateTime checkServerInfoTime;
        public DateTime checkReconnectionServerTime;

        private void Start()
        {
            Connect();
        }

        [Button]
        public void Test()
        {
            onTestAck = (error) => Debug.LogFormat("TestAck: {0}", error);
            Connect();
            proxy.TestReq(0);
        }

        [Button]
        public void Test2()
        {
            onTest2Ack = (error, testInt, testString) => Debug.LogFormat("Test2Ack: {0}, {1}, {2}", error, testInt, testString);
            Connect();
            proxy.Test2Req(1, 125, "125");
        }

        private void Update()
        {
            if(Client != null)
            {
                try
                {
                    Client.FrameMove();
                }
                catch(Exception e)
                {
                    Debug.LogError("GameServer: " + e.Message);
                }
            }
        }

        private void OnDestroy()
        {
            if (Client != null)
                Client.Disconnect();
        }

        // ���������� ������ ������ ������ִ��� Ȯ���� 
        public void Connect()
        {            
            if (Client == null)
            {
                Client = new AIG_Client();
            }

            if (!Client.IsConnected)
            {
                Client.InitClient();

                Client.AttachProxy(proxy);
                Client.AttachStub(stub);

                // Stub���� �޼����� �޾����� ID������ ȣ��� �Լ����� �׼ǿ� ����
                stub.TestAck = TestAck;
                stub.Test2Ack = Test2Ack;
                stub.TransformSyncAck = TransformSyncAck;
                stub.PlayerJoinSyncAck = PlayerJoinAck;
                stub.PlayerQuitSyncAck = PlayerQuitAck;

                stub.StartMatchingAck = StartMatchingAck;
                stub.OnFindMatchAck = FindMatchAck;
                stub.OnStopMatchingAck = StopMatchAck;

                stub.GameReadySyncAck = GameReadySyncAck;
                stub.PoolableSpawnSyncAck = PoolableSpawnSyncAck;
                stub.PoolablePushSyncAck = PoolablePushSyncAck;
                stub.MultipleTransformSyncAck = MutipleTransformSyncAck;
            }
        }

        /*
         * Ÿ ���� Ȥ�� �������� Rmi�� �޾�����
         * ���������� �����ϴ°�
         * �� ����Ӹ��ƴ϶� �ٸ����� �׼��� �����ص� ��
         */
        #region �޼��� �޾����� �׼�

        public Action<int> onTestAck;
        private bool TestAck(int Error)
        {
            if(Error < 0)
            {
                Debug.LogError("TestAck: " + Error);
                // ������ �����ص� �� �Լ��ȿ� �������� �� �Լ��� Stub�� ����ƴٴ� ���̱⶧����
                // ������ true�� ��ȯ�ؾ���
                return true;
            }

            onTestAck?.Invoke(Error);
            return true;
        }

        public Action<int, int, string> onTest2Ack;
        private bool Test2Ack(int error, int testInt, string testString)
        {
            if(error < 0)
            {
                Debug.LogError("Test2Ack: " + error);
                return true;
            }

            onTest2Ack?.Invoke(error, testInt, testString);
            return true;
        }

        #region Ʈ������ ��ũ �׽�Ʈ

        public Action<string, Vector3, Quaternion> onPlayerJoinAck;
        private bool PlayerJoinAck(int error, string userID, Vector3 pos, Quaternion rot)
        {
            if (error < 0)
            {
                Debug.LogError("PlayerJoinAck: " + error);
                return true;
            }

            onPlayerJoinAck?.Invoke(userID, pos, rot);
            return true;
        }

        public Action<string> onPlayerQuitAck;
        private bool PlayerQuitAck(int error, string userID)
        {
            if (error < 0)
            {
                Debug.LogError("PlayerQuitAck: " + error);
                return true;
            }

            onPlayerQuitAck?.Invoke(userID);
            return true;
        }
        #endregion

        #region ��Ī
        public Action onStartMatchingAck;
        private bool StartMatchingAck(int error)
        {
            if (error < 0)
            {
                Debug.LogError("StartMatchingAck: " + error);
                return true;
            }

            onStartMatchingAck?.Invoke();
            return true;
        }

        public Action<string, string> onFindMatchAck;
        private bool FindMatchAck(int error, string nick, string iconID)
        {
            if (error < 0)
            {
                Debug.LogError("FindMatchAck: " + error);
                return true;
            }

            onFindMatchAck?.Invoke(nick, iconID);
            return true;
        }

        public Action onStopMatchAck;
        private bool StopMatchAck(int error)
        {
            if (error < 0)
            {
                Debug.LogError("FindMatchAck: " + error);
                return true;
            }

            onStopMatchAck?.Invoke();
            return true;
        }
        #endregion

        #region ��Ʋ��
        // ������ PVP���� ���� ���ɼ��� �ִ°͵��̶� PvpManager�� ���� ���ϰ� ���ʿ� ������
        public Action<int, Vector3, Vector3, Quaternion> onTransformSyncAck;
        private bool TransformSyncAck(int error, int objectID, Vector3 pos, Vector3 velocity, Quaternion rot)
        {
            if (error < 0)
            {
                Debug.LogError("TransformSyncAck: " + error);
                return true;
            }

            onTransformSyncAck?.Invoke(objectID, pos, velocity, rot);
            return true;
        }

        public Action<string> onGameReadyAck;
        private bool GameReadySyncAck(int error, string userID)
        {
            if (error < 0)
            {
                Debug.LogError("GameReadySyncAck: " + error);
                return true;
            }

            onGameReadyAck?.Invoke(userID);
            return true;
        }

        public Action<int, string> onPoolableSpawnAck;
        private bool PoolableSpawnSyncAck(int error, int syncID, string poolID)
        {
            if (error < 0)
            {
                Debug.LogError("PoolableSpawnSyncAck: " + error);
                return true;
            }

            var trSync = PoolManager.instance.Pop(poolID).GetComponent<TransformSync>();
            trSync.id = syncID;
            trSync.isAvatar = true;
            //SyncObjectContainer.instance.allTransformSyncsList.Add(trSync);

            ParticleSystem particle = trSync.GetComponent<ParticleSystem>();
            if (particle != null)
                particle.Play();

            trSync.gameObject.SetActive(true);

            onPoolableSpawnAck?.Invoke(syncID, poolID);
            return true;
        }

        public Action<int, string> onPoolablePushAck;
        private bool PoolablePushSyncAck(int error, int syncID, string poolID)
        {
            if (error < 0)
            {
                Debug.LogError("PoolablePushSyncAck: " + error);
                return true;
            }

            var trSync = SyncObjectContainer.instance.allTransformSyncsList.Find(t => t.id == syncID);
            if(trSync != null)
            {
                trSync.gameObject.SetActive(false);
                PoolManager.instance.Push(trSync.GetComponent<Poolable>());
            }

            onPoolablePushAck?.Invoke(syncID, poolID);
            return true;
        }

        // ���� Ʈ������ ����ȭ
        public Action<List<TransformSyncReceived>> onMultipleTransformReceiveAck;
        private bool MutipleTransformSyncAck(int error, List<TransformSyncReceived> syncList)
        {
            if(error < 0)
            {
                Debug.LogError("MutipleTransformSyncAck: " + error);
                return true;
            }

            onMultipleTransformReceiveAck?.Invoke(syncList);
            return true;
        }
        #endregion
    }
    #endregion
}
