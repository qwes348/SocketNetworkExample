using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using NGNet;
using Newtonsoft.Json.Linq;

namespace Jamong.Server
{
    [Serializable]
    public class Client
    {
        public string ClientID { get => _clientID; set => _clientID = value; }
        private string _clientID;

        private byte[] bufferRecv = new byte[BasicType.MAX_PACKET_SIZE];
        private int buffRecvSize = 0;
        private Socket socket = null;

        public bool IsConnected { 
            get
            {
                return socket != null && socket.Connected;
            }
        }

        // Send, Receive Queue
        private Queue<JsonMessage> queueSend = null;
        private Queue<JsonMessage> queueReceive = null;

        private bool isSendComplete = true;

        // Critical section
        // https://www.csharpstudy.com/Threads/lock.aspx => 용도 설명
        private object csSend = null;
        private object csReceive = null;

        // Rmi?? Proxy?? stub?? => 개념설명 https://minisp.tistory.com/12
        // 보낼때(Caller)
        private List<RmiProxy> listRmiProxy = new List<RmiProxy>();
        // 받았을때(Executor)
        private List<RmiStub> listRmiStub = new List<RmiStub>();        

        // 콜백
        private AsyncCallback asyncCallbackSend = null;
        private AsyncCallback asyncCallbackRecv = null;

        /*
        TCP 패킷의 순서 번호
        : 이를 활용하여 중복패킷 콜백시 체크하여 접속을 끊어 버린다.
        */
        private Int32 SequenceNumSend = 0;
        private Int32 SequenceNumRecv = 0;
        private Int32 SequenceNumFailCount = 0;

        public Client()
        {
            bufferRecv = new byte[BasicType.MAX_PACKET_SIZE];
            csSend = new object();
            csReceive = new object();
            queueSend = new Queue<JsonMessage>();
            queueReceive = new Queue<JsonMessage>();

            asyncCallbackSend = new AsyncCallback(OnCallbackSend);
            asyncCallbackRecv = new AsyncCallback(OnCallbackReceive);
        }

        public void InitClient()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(GameURL.PVP_IP, GameURL.PVP_PORT);

            if(socket.Connected)
            {
                Debug.Log("Client Connected");
            }
            else
            {
                Debug.LogError("Client Connect Fail");
            }

            //if(DBServerManager.instance != null && DBServerManager.instance.CurrentUserInfo != null)
            //{
            //    ClientID = DBServerManager.instance.CurrentUserInfo.account_uuid;
            //}

            // 한번만 이렇게 호출해주면 콜백에서 또 receive대기하니까 안끊기고 받아오는듯
            BeginReceive();
        }

        public void FrameMove()
        {
            if (!IsConnected)
                return;

            lock(csSend)
            {
                if (queueSend.Count > 0 && isSendComplete)
                    BeginSend();
            }

            List<JsonMessage> vecReceive = new List<JsonMessage>();

            lock(csReceive)
            {
                while (queueReceive.Count > 0)
                {
                    vecReceive.Add(queueReceive.Dequeue());
                }                
            }

            if(vecReceive.Count > 0)
            {
                foreach(var msg in vecReceive)
                {
                    // 이 메세지가 RPC호출이라면 true가 됨
                    bool isStub = false;
                    foreach(var stub in listRmiStub)
                    {
                        try
                        {// 모든 stub를 돌면서 해당 서버기능에 정의된 RPC인지 확인
                            if (stub.ProcessReceivedMessage(msg))
                            {
                                isStub = true;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("stub.ProcessReceivedMessage(msg) " + e.Message);
                        }
                    }

                    // 모든 Stub에서 RPC함수가 없었다면 여기서 메세지에 ID에따라 처리
                    // 엔젤서버는 ClientConnectionParam 클래스를 이용해 액션으로 처리했음
                    if (!isStub)
                    {
                        switch (msg.ID)
                        {
                            default:
                                isStub = true;
                                break;
                        }
                    }

                    if(!isStub && listRmiStub.Count > 0)
                    {
                        // Stub가 비어있는것도 아닌데 메세지에서 의도한 함수를 못찾겠다면 들어옴
                        Debug.Log("function that a user did not create has been called. : listRmiStub.Count > 0");
                    }
                }
            }
        }


        /*
         * 패킷전송
         * 큐가 비어 있을 경우에는 큐에 추가한 뒤 바로 Send 하고
         * 데이터가 들어 있을 경우에는 새로 추가만 한다.
         */
        public void RmiSend(Int32 packetID, JsonMessage.TargetEnum target, JsonMessage packet)
        {
            packet.ID = packetID;
            //packet.RmiContextValue = (byte)rmiContext;
            packet.ClientID = ClientID;
            packet.Target = target;
            packet.WriteEnd();

            lock (csSend)
            {
                queueSend.Enqueue(packet);
            }
        }

        /*
         * 클라이언트 Tick 에서 호출해주어야 한다.
         * Recv 된 패킷이 queue 되어 있을 경우 1 frame 당 1개의 packet 을 OnPacketReceive() 을 통해 전달 해준다
         */
        private void BeginSend()
        {
            try
            {
                if(!IsConnected)
                {
                    Debug.LogError("BeginSend() : IsConnected == false");
                    return;
                }

                JsonMessage msg = null;

                lock(csSend)
                {
                    if (queueSend.Count == 0)
                        return;

                    // peek => Dequeue했을때와 같은 값을 가져오지만 Queue에서 제거하지는 않는다.
                    msg = queueSend.Peek();

                    isSendComplete = false;
                }

                ++SequenceNumSend;
                //msg.SequenceNum = SequenceNumSend;

                try
                {                    
                    socket.BeginSend(msg.buffer, 0, msg.Length, SocketFlags.None, asyncCallbackSend, socket);
                }
                catch(Exception ex)
                {
                    Debug.LogError("Error? " + ex.Message);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("BeginSend() exception : " + e.Message);

                try
                {
                    if (socket.Connected)
                        ShutDown();                    
                }
                catch(Exception ex)
                {
                    Debug.LogError("BeginSend() socket shutdown Exception " + ex.ToString());
                }
            }
        }

        /*
         * Socket.BeginSend() 에 바인딩 되어 Send 완료시 호출
         * P.S : host thread 에서 호출됨
         */
        private void OnCallbackSend(IAsyncResult ar)
        {
            try
            {
                Socket socketTemp = (Socket)ar.AsyncState;

                int sendSize = socketTemp.EndSend(ar);

                Debug.LogFormat("보낸데이터 길이 : {0}", sendSize);               

                lock (csSend)
                {
                    if(!isSendComplete)
                    {
                        var packet = queueSend.Dequeue();

                        if(packet.Length != sendSize)
                        {
                            Debug.LogError("OnCallbackSend : packet.position != sendSize...?");
                        }

                        //string data = "";
                        //for (int i = 0; i < packet.Length; i++)
                        //{
                        //    data += " " + packet.buffer[i].ToString();
                        //}

                        Debug.LogFormat("보낸 데이터: {0}", packet.jsonBuffer.ToString(Newtonsoft.Json.Formatting.Indented));
                        //Debug.LogFormat("보낸 데이터: {0}", Encoding.UTF8.GetString(packet.buffer));
                    }
                    else
                    {
                        Debug.LogError("OnCallbackSend : queSend.Count == 0");
                    }

                    isSendComplete = true;
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("OnCallbackSend exception : " + ex.ToString());

                try
                {
                    if (socket.Connected)
                        socket.Shutdown(SocketShutdown.Both);                    
                }
                catch (Exception)
                {
                    Debug.LogError("OnCallbackSend() socket shutdown Exception " + ex.ToString());
                }
            }
        }

        public void BeginReceive()
        {
            try
            {
                socket.BeginReceive(bufferRecv, buffRecvSize, bufferRecv.Length, SocketFlags.None, asyncCallbackRecv, socket);
                //socket.BeginReceive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None, asyncCallbackRecv, socket);
            }
            catch(Exception e)
            {
                //Debug.LogError(e);
                Debug.LogErrorFormat("Receive Exception: {0}", e.Message);
                ShutDown();
            }
        }


        /*
         * Socket.BeginReceive() 바인딩 되어 Packet 을 받게 되면 호출
         * P.S : host thread 에서 호출됨
         */
        private void OnCallbackReceive(IAsyncResult ar)
        {
            try
            {
                Socket socketTemp = (Socket)ar.AsyncState;

                if (socketTemp == null)
                    return;

                int readSize = socketTemp.EndReceive(ar);

                Debug.LogFormat("받은 데이터 길이: {0}", readSize);

                //Debug.Log(Encoding.UTF8.GetString(bufferRecv, BasicType.HEADSIZE, readSize));

                if (readSize > 0)
                {
                    buffRecvSize += readSize;

                    /*
                    패킷이 나누어져서 오거나
                    2개 이상의 패킷이 하나로 올수 있어서 체크한다(네이글 알골리즘)
                   */

                    while (buffRecvSize >= sizeof(int))
                    {
                        int packetLength = 0;
                        packetLength = BitConverter.ToInt32(bufferRecv, 0);

                        if(buffRecvSize >= packetLength)
                        {
                            JsonMessage msgRecv = new JsonMessage();
                            //msgRecv.buffer = bufferRecv;
                            Buffer.BlockCopy(bufferRecv, 0, msgRecv.buffer, 0, packetLength);

                            buffRecvSize -= packetLength;
                            if(buffRecvSize >= 1)
                            {
                                Buffer.BlockCopy(bufferRecv, packetLength, bufferRecv, 0, buffRecvSize);
                            }

                            // 암호화했다면 이 사이에서 복호화
                            msgRecv.ReadEnd();
                            Debug.LogFormat("받은 데이터: {0}", msgRecv.jsonBuffer.ToString(Newtonsoft.Json.Formatting.Indented));

                            lock (csReceive)
                            {
                                // 클라 쓰레드로 전달
                                queueReceive.Enqueue(msgRecv);
                            }
                        }   
                        else
                        {
                            // 온전한 패킷을 구성하기에 충분한 데이터가 없음. loop를 빠져나간다.
                            break;
                        }
                    }
                }
                else if (readSize == 0)
                {
                    Debug.Log("OnCallbackRecv() ReadSize == 0 : Socket Close");

                    if (socket.Connected)
                        ShutDown();                    

                    return;
                }

                string data = string.Empty;
                for (int i = 0; i < readSize; i++)
                {
                    data += " " + bufferRecv[i].ToString();
                }
                //Debug.LogFormat("받은 데이터: {0}", data);

                if (IsConnected)
                {
                    socket.BeginReceive(bufferRecv, buffRecvSize, bufferRecv.Length - buffRecvSize, SocketFlags.None, asyncCallbackRecv, socket);
                    //socket.BeginReceive(bufferRecv, 0, bufferRecv.Length, SocketFlags.None, asyncCallbackRecv, socket);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("OnCallbackRecv exception : " + ex.ToString());

                try
                {
                    if (socket.Connected)
                        ShutDown();                    
                }
                catch (Exception exSub)
                {
                    Debug.LogError("OnCallbackRecv socket shutdown exception : " + exSub.ToString());
                }
            }
        }

        public void Disconnect()
        {
            ShutDown();
        }

        private void ShutDown()
        {
            if (socket == null)
                return;
            if (!socket.Connected)
                return;

            try
            {
                Debug.Log("shutdown");
                socket.Shutdown(SocketShutdown.Both);
                socket = null;
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat("Shutdown Exception: {0}", e.Message);
                socket = null;
            }
        }

        public void AttachProxy(RmiProxy proxy)
        {
            proxy.m_core = this;
            listRmiProxy.Add(proxy);
        }

        public void AttachStub(RmiStub stub)
        {
            listRmiStub.Add(stub);
        }
    }



    // 이건 엔젤서버에서 복사해왔는데 아직 쓰진않음
    /// <summary>
    ///
    /// 서버 접속시 사용할 파라메터
    /// 변수
    ///      hostname : 서버에 접속할 IP 또는 호스트네임
    ///      port : 접속할 포트
    ///      OnJoinServerComplete : 접속 성공시 호출 될 함수 포인터   
    ///      OnLeaveServer : 서버 접속 종료시 호출 될 함수 포인터
    ///      OnReceiveMessage : Packet receive 를 받는 함수(NIDL 을 사용하지 않고 직접 처리 할 때 사용 하면됨)
    ///
    /// </summary>
    //public class ClientConnectionParam
    //{
    //    public string hostname = "127.0.0.1";
    //    public int port = 10000;



    //    public delegate void OnJoinServerCompleteDelegate(ErrorType errorType);
    //    public OnJoinServerCompleteDelegate OnJoinServerComplete = null; //delegate ( ErrorType errorType ){};        
    //    public delegate void OnPacketReceiveDelegate(Message msg);
    //    public OnPacketReceiveDelegate OnReceiveMessage = null; //delegate ( ErrorType errorType ){};
    //    public delegate void OnLeaveServerDelegate(ErrorType errorType);
    //    public OnLeaveServerDelegate OnLeaveServer = null; //delegate ( ErrorType errorType ){};

    //    public bool IsValid
    //    {
    //        get
    //        {
    //            if (OnJoinServerComplete == null ||
    //                OnLeaveServer == null
    //                )
    //                return false;
    //            return true;
    //        }
    //    }
    //}
}
