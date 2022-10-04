using NGNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIGears.Server
{
    // �޼����� �ް� ������ �׼��� ã�� ȣ�����ִ°�
    public class GameServer_stub : NGNet.RmiStub
    {
        // �Ķ���Ͱ� ���°��
        public delegate bool TestDelegate(int Error);
        public TestDelegate TestAck = delegate (int Error)
        {
            // �ƹ� �׼��� ��ϵ��� �ʾҴٸ� false�� ������
            return false;
        };

        #region ��������Ʈ
        #region ���� �׽�Ʈ
        // �Ķ���Ͱ� �ִ°��
        public delegate bool Test2Delegate(int error, int testNum, string testString);
        public Test2Delegate Test2Ack = delegate (int error, int testNum, string testString)
        {
            return false;
        };


        #endregion

        #region Ʈ������ ��ũ �׽�Ʈ
        public delegate bool PlayerJoinDelegate(int error, string userID, Vector3 pos, Quaternion rot);
        public PlayerJoinDelegate PlayerJoinSyncAck = delegate (int error, string userID, Vector3 pos, Quaternion rot)
        {
            return false;
        };

        public delegate bool PlayerQuitDelegate(int error, string userID);
        public PlayerQuitDelegate PlayerQuitSyncAck = delegate (int error, string userID)
        {
            return false;
        };
        #endregion

        #region ��Ī
        // ��Ī ����
        public delegate bool StartMatchingDelegate(int error);
        public StartMatchingDelegate StartMatchingAck = delegate (int error)
        {
            return false;
        };

        // ��Ī ����
        public delegate bool S2C_OnFindMatchDelegate(int error, string userNick, string iconID);
        public S2C_OnFindMatchDelegate OnFindMatchAck = delegate (int error, string userNick, string iconID)
        {
            return false;
        };

        // ��Ī �ߴ�
        public delegate bool S2C_OnSucessStopMatchingDelegate(int error);
        public S2C_OnSucessStopMatchingDelegate OnStopMatchingAck = delegate (int error)
        {
            return false;
        };

        public delegate bool PvpLoadSceneCompleteDelegate(int error);
        public PvpLoadSceneCompleteDelegate PvpLoadSceneCompleteAck = delegate (int error)
        {
            return false;
        };
        #endregion

        #region ��Ʋ��
        public delegate bool RoundSallyAgentSyncDelegate(int error, int robotIndex, int slotIndex);
        public RoundSallyAgentSyncDelegate RoundSallyAgentSyncAck = delegate (int error, int robotIndex, int slotIndex)
        {
            return false;
        };

        public delegate bool TransformSyncDelegate(int error, int objectID, Vector3 pos, Vector3 velocity, Quaternion rot);
        public TransformSyncDelegate TransformSyncAck = delegate (int error, int objectID, Vector3 pos, Vector3 velocity, Quaternion rot)
        {
            return false;
        };

        public delegate bool GameReadySyncDelegate(int error, string userID);
        public GameReadySyncDelegate GameReadySyncAck = delegate (int error, string userID)
        {
            return false;
        };

        public delegate bool PoolableSpawnSyncDelegate(int error, int syncID, string poolID);
        public PoolableSpawnSyncDelegate PoolableSpawnSyncAck = delegate (int error, int syncID, string poolID)
        {
            return false;
        };

        public delegate bool PoolablePushSyncDelegate(int error, int syncID, string poolID);
        public PoolablePushSyncDelegate PoolablePushSyncAck = delegate (int error, int syncID, string poolID)
        {
            return false;
        };

        // ���� Ʈ������ ��ũ
        public delegate bool MultipleTransformSyncDelegate(int error, List<TransformSyncReceived> syncList);
        public MultipleTransformSyncDelegate MultipleTransformSyncAck = delegate (int error, List<TransformSyncReceived> syncList)
        {
            return false;
        };
        #endregion

        #endregion

        // �޼����� ��ŶID������ ������ RMI�Լ��� ȣ���� => �̰Ŵ� �Ⱦ�
        public override bool ProcessReceivedMessage(Message msg)
        {
            return false;
        }


        /// <summary>
        /// �޼����� ��ŶID������ ������ RMI�Լ��� ȣ����
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override bool ProcessReceivedMessage(JsonMessage msg)
        {
            switch (msg.ID)
            {
                #region �׽�Ʈ��
                case GameServer_RMI.Rmi_TestAck:
                    ProcessReceivedMessage_Test(msg);
                    break;
                case GameServer_RMI.Rmi_Test2Ack:
                    ProcessReceivedMessage_Test2(msg);
                    break;
                case GameServer_RMI.Rmi_TransformSync:
                    ProcessReceivedMessage_TransformSync(msg);
                    break;
                case GameServer_RMI.Rmi_OnPlayerJoin:
                    ProcessReceivedMessage_PlayerJoin(msg);
                    break;
                case GameServer_RMI.Rmi_OnPlayerQuit:
                    ProcessReceivedMessage_PlayerQuit(msg);
                    break;
                #endregion

                #region ��Ī
                case GameServer_RMI.S2C_MatchingStart:
                    ProcessReceivedMessage_S2CMatchingStart(msg);
                    break;
                case GameServer_RMI.S2C_OnFindMatch:
                    ProcessReceivedMessage_S2CFindMatch(msg);
                    break;
                case GameServer_RMI.S2C_SuccessStopMatch:
                    ProcessReceiveMessage_S2CStopMatching(msg);
                    break;
                case GameServer_RMI.Rmi_PvpLoadSceneComplete:
                    ProcessReceiveMessage_PvpLoadSceneComplete(msg);
                    break;
                #endregion

                #region ��Ʋ��
                case GameServer_RMI.Rmi_MultipleTransformSync:
                    ProcessReceiveMessage_MultipleTransformSync(msg);
                    break;
                case GameServer_RMI.Rmi_RoundSallyAgentSync:
                    ProcessReceiveMessage_RoundSallyAgentSync(msg);
                    break;
                case GameServer_RMI.Rmi_PoolableSpawnSync:
                    ProcessReceiveMessage_PoolableSpawnSync(msg);
                    break;
                case GameServer_RMI.Rmi_PoolablePushSync:
                    ProcessReceiveMessage_PoolablePushSync(msg);
                    break;
                #endregion
                default:
                    return false;
            }
            return true;
        }


        // ProcessReceivedMessage--------------------------------------------------------------------------------------------

        #region ���� �׽�Ʈ
        private void ProcessReceivedMessage_Test(JsonMessage msg)
        {
            //MessageMarshal.Read(msg, out int Error);
            msg.Read("error", out int error);

            bool _ret = TestAck(error);
            // ��ϵ� �׼��� ���°��
            if(_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceivedMessage_Test2(JsonMessage msg)
        {
            msg.Read("error", out int error);
            msg.Read("testNum", out int testNum);
            msg.Read("testString", out string testString);

            bool _ret = Test2Ack(error, testNum, testString);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }
        #endregion

        #region Ʈ������ ��ũ �׽�Ʈ

        private void ProcessReceivedMessage_PlayerJoin(JsonMessage msg)
        {
            msg.Read("error", out int error);
            msg.Read("userID", out string userID);
            msg.Read("pos", out Vector3 pos);
            msg.Read("rot", out Quaternion rot);

            bool _ret = PlayerJoinSyncAck(error, userID, pos, rot);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceivedMessage_PlayerQuit(JsonMessage msg)
        {
            msg.Read("error", out int error);
            msg.Read("userID", out string userID);

            bool _ret = PlayerQuitSyncAck(error, userID);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }
        #endregion

        #region ��Ī
        private void ProcessReceivedMessage_S2CMatchingStart(JsonMessage msg)
        {
            int error;

            try
            {
                msg.Read("error", out error);
            }
            catch(Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = StartMatchingAck(error);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceivedMessage_S2CFindMatch(JsonMessage msg)
        {
            int error;
            string nick;
            string iconID;

            try
            {
                msg.Read("error", out error);
                msg.Read("nick", out nick);
                msg.Read("iconID", out iconID);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = OnFindMatchAck(error, nick, iconID);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceiveMessage_S2CStopMatching(JsonMessage msg)
        {
            int error;

            try
            {
                msg.Read("error", out error);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = OnStopMatchingAck(error);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceiveMessage_PvpLoadSceneComplete(JsonMessage msg)
        {
            int error;

            try
            {
                msg.Read("error", out error);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = PvpLoadSceneCompleteAck(error);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }
        #endregion

        #region ��Ʋ��
        private void ProcessReceiveMessage_RoundSallyAgentSync(JsonMessage msg)
        {
            int error;
            int robotIndex;
            int slotIndex;

            try
            {
                msg.Read("error", out error);
                msg.Read("robotIndex", out robotIndex);
                msg.Read("slotIndex", out slotIndex);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = RoundSallyAgentSyncAck(error, robotIndex, slotIndex);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceivedMessage_TransformSync(JsonMessage msg)
        {
            int error;
            int objectID;
            Vector3 pos;
            Vector3 velocity;
            Quaternion rot;

            try
            {
                msg.Read("error", out error);
                msg.Read("objectID", out objectID);
                msg.Read("pos", out pos);
                msg.Read("velocity", out velocity);
                msg.Read("rot", out rot);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = TransformSyncAck(error, objectID, pos, velocity, rot);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceiveMessage_PoolableSpawnSync(JsonMessage msg)
        {
            int error;
            int syncID;
            string poolID;

            try
            {
                msg.Read("error", out error);
                msg.Read("syncID", out syncID);
                msg.Read("poolID", out poolID);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = PoolableSpawnSyncAck(error, syncID, poolID);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        private void ProcessReceiveMessage_PoolablePushSync(JsonMessage msg)
        {
            int error;
            int syncID;
            string poolID;

            try
            {
                msg.Read("error", out error);
                msg.Read("syncID", out syncID);
                msg.Read("poolID", out poolID);
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = PoolablePushSyncAck(error, syncID, poolID);
            if (_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }

        // ���� Ʈ������ ��ũ
        private void ProcessReceiveMessage_MultipleTransformSync(JsonMessage msg)
        {
            int error;
            int count;

            List<TransformSyncReceived> receivedTransformSyncList = new List<TransformSyncReceived>();

            try
            {
                msg.Read("error", out error);
                // ����Ʈ ���� Ȯ��
                msg.Read("count", out count);

                // ������ŭ ����ȭ ��ü ����
                for (int i = 0; i < count; i++)
                {
                    var tr = new TransformSyncReceived();
                    msg.Read("objectID" + i, out tr.objectID);
                    // ID�� ������� null�� ��ü
                    if (tr.objectID < 0)
                        continue;

                    msg.Read("pos" + i, out tr.position);
                    msg.Read("rot" + i, out tr.rotation);
                    msg.Read("syncVelocity" + i, out tr.syncVelocity);
                    if(tr.syncVelocity)
                    {
                        msg.Read("velocity" + i, out tr.velocity);
                    }

                    receivedTransformSyncList.Add(tr);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
                return;
            }

            bool _ret = MultipleTransformSyncAck(error, receivedTransformSyncList);
            if(_ret == false)
                Debug.LogError("Error: RMI function that a user did not create has been called.");
        }
        #endregion
    }
}
