using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGNet;

namespace AIGears.Server
{
    // 리퀘스트를 보내는곳
    public class GameServer_proxy : RmiProxy
    {
        #region 심플 테스트
        public void TestReq(int error)
        {
            //Message msg = new Message();
            //MessageMarshal.Write(msg, error);
            //RmiSend(rmiContext, GameServer_RMI.Rmi_TestAck, msg);

            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            RmiSend(GameServer_RMI.Rmi_TestAck, msg, JsonMessage.TargetEnum.AllPlayers);
        }

        public void Test2Req(int error, int testNum, string testString)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("testNum", testNum);
            msg.Write("testString", testString);
            RmiSend(GameServer_RMI.Rmi_Test2Ack, msg, JsonMessage.TargetEnum.AllPlayers);
        }
        #endregion

        #region 트랜스폼 싱크 테스트

        public void PlayerJoinReq(int error, string userID, Vector3 pos, Quaternion rot, JsonMessage.TargetEnum target = JsonMessage.TargetEnum.AllPlayersInTheRoom)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("userID", userID);
            msg.Write("pos", pos);
            msg.Write("rot", rot);
            RmiSend(GameServer_RMI.Rmi_OnPlayerJoin, msg, target);
        }

        public void PlayerQuitReq(int error, string userID, JsonMessage.TargetEnum target = JsonMessage.TargetEnum.AllPlayersInTheRoom)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("userID", userID);
            RmiSend(GameServer_RMI.Rmi_OnPlayerQuit, msg, target);
        }
        #endregion


        #region 매칭
        public void StartMatchingReq(int error, string nick, string iconID)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("nick", nick);
            msg.Write("iconID", iconID);
            RmiSend(GameServer_RMI.C2S_StartMatchingReq, msg, JsonMessage.TargetEnum.None);
        }

        public void StopMatchingReq(int error)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            RmiSend(GameServer_RMI.C2S_StopMatchingReq, msg, JsonMessage.TargetEnum.None);
        }

        public void PvpLoadSceneCompelteReq(int error)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            RmiSend(GameServer_RMI.Rmi_PvpLoadSceneComplete, msg, JsonMessage.TargetEnum.OtherPlayersInTheRoom);
        }
        #endregion

        #region 배틀씬
        public void RoundSallyAgentSyncReq(int error, int robotIndex, int slotIndex)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("robotIndex", robotIndex);
            msg.Write("slotIndex", slotIndex);
            RmiSend(GameServer_RMI.Rmi_RoundSallyAgentSync, msg, JsonMessage.TargetEnum.OtherPlayersInTheRoom);
        }

        public void TransformSyncReq(int error, int objectID, Vector3 pos, Vector3 velocity, Quaternion rot, JsonMessage.TargetEnum target = JsonMessage.TargetEnum.OtherPlayersInTheRoom)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("objectID", objectID);
            msg.Write("pos", pos);
            msg.Write("velocity", velocity);
            msg.Write("rot", rot);
            RmiSend(GameServer_RMI.Rmi_TransformSync, msg, target);
        }

        public void MultipleTransformSyncReq(int error, List<TransformSync> transformList, JsonMessage.TargetEnum target = JsonMessage.TargetEnum.OtherPlayersInTheRoom)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("count", transformList.Count);

            for (int i = 0; i < transformList.Count; i++)
            {
                var tr = transformList[i];

                if (tr == null)
                {
                    // null이라면 음수로 채워줌
                    msg.Write("objectID" + i, -1);
                    continue;
                }

                msg.Write("objectID" + i, tr.id);
                msg.Write("pos" + i, tr.transform.position);
                msg.Write("rot" + i, tr.transform.rotation);
                msg.Write("syncVelocity" + i, tr.syncVelocity && tr.rb != null);
                if (tr.syncVelocity)
                {
                    msg.Write("velocity" + i, tr.rb.velocity);
                }
            }

            RmiSend(GameServer_RMI.Rmi_MultipleTransformSync, msg, target);
        }

        public void PoolableSpawnSyncReq(int error, int syncID, string poolID, JsonMessage.TargetEnum target = JsonMessage.TargetEnum.OtherPlayersInTheRoom)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("syncID", syncID);
            msg.Write("poolID", poolID);
            RmiSend(GameServer_RMI.Rmi_PoolableSpawnSync, msg, target);
        }

        public void PoolablePushSyncReq(int error, int syncID, string poolID)
        {
            JsonMessage msg = new JsonMessage();
            msg.Write("error", error);
            msg.Write("syncID", syncID);
            msg.Write("poolID", poolID);
            RmiSend(GameServer_RMI.Rmi_PoolablePushSync, msg, JsonMessage.TargetEnum.OtherPlayersInTheRoom);
        }
        #endregion
    }
}
