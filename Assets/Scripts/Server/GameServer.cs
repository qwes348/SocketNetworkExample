using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGNet;
using System;
using NaughtyAttributes;

namespace Jamong.Server
{
    public class GameServer : Singleton2<GameServer>
    {
        public Client Client { get; private set; }

        public GameServer_proxy proxy = new GameServer_proxy();
        private GameServer_stub stub = new GameServer_stub();

        //public int port;

        public bool IsConnectServer { get => Client != null && Client.IsConnected; }

        public Action<Client> ActionOnConnected;

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
            if (Client != null)
            {
                try
                {
                    Client.FrameMove();
                }
                catch (Exception e)
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

        // 원본에서는 유저가 서버와 연결돼있는지 확인함 
        public void Connect()
        {
            if (Client == null)
            {
                Client = new Client();
            }

            if (!Client.IsConnected)
            {
                Client.InitClient();

                Client.AttachProxy(proxy);
                Client.AttachStub(stub);

                // Stub에서 메세지를 받았을때 ID에따라 호출될 함수들을 액션에 연결
                stub.TestAck = TestAck;
                stub.Test2Ack = Test2Ack;

                stub.StartMatchingAck = StartMatchingAck;
                stub.OnFindMatchAck = FindMatchAck;
                stub.OnStopMatchingAck = StopMatchAck;
                stub.PvpLoadSceneCompleteAck = PvpLoadSceneCompleteAck;
                stub.OnLeaveMatchAck = LeaveMatchAck;
                stub.OnLeaveMatchOtherAck = LeaveMatchOtherAck;

                stub.GameReadySyncAck = GameReadySyncAck;
                stub.PoolableSpawnSyncAck = PoolableSpawnSyncAck;
                stub.PoolablePushSyncAck = PoolablePushSyncAck;
                stub.MultipleTransformSyncAck = MutipleTransformSyncAck;

                stub.OnSyncIdReceiveAck = SyncidReceiveAck;
            }
        }

        /*
         * 타 유저 혹인 서버에게 Rmi를 받았을때
         * 최종적으로 도달하는곳
         * 꼭 여기뿐만아니라 다른곳에 액션을 정의해도 됨
         */
        #region 메세지 받았을때 액션

        public Action<int> onTestAck;
        private bool TestAck(int Error)
        {
            if (Error < 0)
            {
                Debug.LogError("TestAck: " + Error);
                // 에러가 난다해도 이 함수안에 들어왔으면 이 함수가 Stub에 연결됐다는 것이기때문에
                // 무조건 true를 반환해야함
                return true;
            }

            onTestAck?.Invoke(Error);
            return true;
        }

        public Action<int, int, string> onTest2Ack;
        private bool Test2Ack(int error, int testInt, string testString)
        {
            if (error < 0)
            {
                Debug.LogError("Test2Ack: " + error);
                return true;
            }

            onTest2Ack?.Invoke(error, testInt, testString);
            return true;
        }

        #region 매칭
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

        public Action onOtherPlayerLoadSceneCompleteAck;
        private bool PvpLoadSceneCompleteAck(int error)
        {
            if (error < 0)
            {
                Debug.LogError("PvpLoadSceneCompleteAck: " + error);
                return true;
            }

            onOtherPlayerLoadSceneCompleteAck?.Invoke();
            return true;
        }

        public Action onLeaveMatchAck;
        private bool LeaveMatchAck(int error)
        {
            if (error < 0)
            {
                Debug.LogError("LeaveMatchAck: " + error);
                return true;
            }

            onLeaveMatchAck?.Invoke();
            return true;
        }

        public Action onLeaveMatchOtherAck;
        private bool LeaveMatchOtherAck(int error)
        {
            if (error < 0)
            {
                Debug.LogError("LeaveMatchOtherAck: " + error);
                return true;
            }

            onLeaveMatchOtherAck?.Invoke();
            return true;
        }
        #endregion

        #region 배틀씬
        // 이쪽은 PVP말고도 사용될 가능성이 있는것들이라 PvpManager에 구현 안하고 이쪽에 구현함
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

            var jv = PoolManager.instance.Pop(poolID).GetComponent<JamongView>();
            jv.Init(syncID, false);
            //SyncObjectContainer.instance.allTransformSyncsList.Add(trSync);

            ParticleSystem particle = jv.GetComponent<ParticleSystem>();
            if (particle != null)
                particle.Play();

            jv.gameObject.SetActive(true);

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

            var trSync = SyncObjectContainer.instance.allTransformSyncsList.Find(t => t.MyJamongView.ViewID == syncID);
            if (trSync != null)
            {
                trSync.gameObject.SetActive(false);
                PoolManager.instance.Push(trSync.GetComponent<Poolable>());
            }

            onPoolablePushAck?.Invoke(syncID, poolID);
            return true;
        }

        // 다중 트랜스폼 동기화
        public Action<List<TransformSyncReceived>> onMultipleTransformReceiveAck;
        private bool MutipleTransformSyncAck(int error, List<TransformSyncReceived> syncList)
        {
            if (error < 0)
            {
                Debug.LogError("MutipleTransformSyncAck: " + error);
                return true;
            }

            onMultipleTransformReceiveAck?.Invoke(syncList);
            return true;
        }
        #endregion

        #region 기타S2C
        public Action<int> onSyncIdReceiveAck;
        private bool SyncidReceiveAck(int error, int syncId)
        {
            if (error < 0)
            {
                Debug.LogError("SyncidReceiveAck: " + error);
                return true;
            }

            onSyncIdReceiveAck?.Invoke(syncId);
            return true;
        }
        #endregion
    }
    #endregion
}
