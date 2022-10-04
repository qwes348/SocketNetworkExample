using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIGears.Server
{
    /*
     * ������ ���� ������Ʈ�� ������ ������ ID�� �����ϴ� Ŭ����
     * 
     * 10000������ ��ȣ�� ������ ������Ʈ�� �������� Ÿ���� �����ϰ� �˾Ƽ� ó����
     */

    public static class GameServer_RMI
    {
        public const int Rmi_TestAck = 1;
        public const int Rmi_Test2Ack = 2;

        #region Ʈ������ ����ȭ �׽�Ʈ��        
        public const int Rmi_OnPlayerJoin = 4;
        public const int Rmi_OnPlayerQuit = 5;
        #endregion

        public const int Rmi_PvpPlayerInfoSync = 6;
        public const int Rmi_PvpLoadSceneComplete = 7;

        #region ��Ʋ��
        public const int Rmi_TransformSync = 3;
        // ����������Ʈ Ʈ������ ����ȭ
        public const int Rmi_MultipleTransformSync = 18;
        public const int Rmi_RoundSallyAgentSync = 8;

        // ���� ���� ����ȭ
        public const int Rmi_GameReadySync = 15;
        // Ǯ��������Ʈ ���� ����ȭ
        public const int Rmi_PoolableSpawnSync = 16;
        // Ǯ�� ������Ʈ ��Ȱ��ȭ ����ȭ
        public const int Rmi_PoolablePushSync = 17;
        #endregion

        /*
         * C2S
         */
        #region ��Ī C2S
        public const int C2S_StartMatchingReq = 10040;
        public const int C2S_StopMatchingReq = 10041;
        #endregion


        /*
         * S2C
         */
        #region ��Ī S2C
        public const int S2C_MatchingStart = 20040;
        public const int S2C_OnFindMatch = 20041;
        public const int S2C_SuccessStopMatch = 20042;
        // 20052
        #endregion
    }
}


// TODO
//10040: ��Ī ť �����ϱ�
//{"EventID":20040,"data":{ "error":0,"message":"��ġ ť �����"}}
//{ "EventID":20041,"data":{ "error":0,"matchId":"0u64fD"","message":"��Ī �Ϸ�"} }

//10041: ��Ī ť ����ϱ�
//{"EventID":20042,"data":{ "error":0,"message":"��Ī ��ҵ�"}

//10050: �� ������
//{"EventID":20051,"data":{ "error":0,"message":"�ڽ��� ���� ����"} }
//{ "EventID":20052,"data":{ "error":0,"message":"��밡 ���� ����"} }
