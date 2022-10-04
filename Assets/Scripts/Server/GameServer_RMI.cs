using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIGears.Server
{
    /*
     * 서버에 보낼 리퀘스트의 종류를 구분할 ID를 보관하는 클래스
     * 
     * 10000번대의 번호를 가지는 리퀘스트는 서버에서 타겟을 무시하고 알아서 처리함
     */

    public static class GameServer_RMI
    {
        public const int Rmi_TestAck = 1;
        public const int Rmi_Test2Ack = 2;

        #region 트랜스폼 동기화 테스트용        
        public const int Rmi_OnPlayerJoin = 4;
        public const int Rmi_OnPlayerQuit = 5;
        #endregion

        public const int Rmi_PvpPlayerInfoSync = 6;
        public const int Rmi_PvpLoadSceneComplete = 7;

        #region 배틀씬
        public const int Rmi_TransformSync = 3;
        // 여러오브젝트 트랜스폼 동기화
        public const int Rmi_MultipleTransformSync = 18;
        public const int Rmi_RoundSallyAgentSync = 8;

        // 게임 레디 동기화
        public const int Rmi_GameReadySync = 15;
        // 풀링오브젝트 생성 동기화
        public const int Rmi_PoolableSpawnSync = 16;
        // 풀링 오브젝트 비활성화 동기화
        public const int Rmi_PoolablePushSync = 17;
        #endregion

        /*
         * C2S
         */
        #region 매칭 C2S
        public const int C2S_StartMatchingReq = 10040;
        public const int C2S_StopMatchingReq = 10041;
        #endregion


        /*
         * S2C
         */
        #region 매칭 S2C
        public const int S2C_MatchingStart = 20040;
        public const int S2C_OnFindMatch = 20041;
        public const int S2C_SuccessStopMatch = 20042;
        // 20052
        #endregion
    }
}


// TODO
//10040: 매칭 큐 진입하기
//{"EventID":20040,"data":{ "error":0,"message":"매치 큐 대기중"}}
//{ "EventID":20041,"data":{ "error":0,"matchId":"0u64fD"","message":"매칭 완료"} }

//10041: 매칭 큐 취소하기
//{"EventID":20042,"data":{ "error":0,"message":"매칭 취소됨"}

//10050: 방 떠나기
//{"EventID":20051,"data":{ "error":0,"message":"자신이 방을 떠남"} }
//{ "EventID":20052,"data":{ "error":0,"message":"상대가 방을 떠남"} }
