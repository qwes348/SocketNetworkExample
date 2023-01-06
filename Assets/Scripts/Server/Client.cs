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
        // https://www.csharpstudy.com/Threads/lock.aspx => �뵵 ����
        private object csSend = null;
        private object csReceive = null;

        // Rmi?? Proxy?? stub?? => ���伳�� https://minisp.tistory.com/12
        // ������(Caller)
        private List<RmiProxy> listRmiProxy = new List<RmiProxy>();
        // �޾�����(Executor)
        private List<RmiStub> listRmiStub = new List<RmiStub>();        

        // �ݹ�
        private AsyncCallback asyncCallbackSend = null;
        private AsyncCallback asyncCallbackRecv = null;

        /*
        TCP ��Ŷ�� ���� ��ȣ
        : �̸� Ȱ���Ͽ� �ߺ���Ŷ �ݹ�� üũ�Ͽ� ������ ���� ������.
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

            // �ѹ��� �̷��� ȣ�����ָ� �ݹ鿡�� �� receive����ϴϱ� �Ȳ���� �޾ƿ��µ�
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
                    // �� �޼����� RPCȣ���̶�� true�� ��
                    bool isStub = false;
                    foreach(var stub in listRmiStub)
                    {
                        try
                        {// ��� stub�� ���鼭 �ش� ������ɿ� ���ǵ� RPC���� Ȯ��
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

                    // ��� Stub���� RPC�Լ��� �����ٸ� ���⼭ �޼����� ID������ ó��
                    // ���������� ClientConnectionParam Ŭ������ �̿��� �׼����� ó������
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
                        // Stub�� ����ִ°͵� �ƴѵ� �޼������� �ǵ��� �Լ��� ��ã�ڴٸ� ����
                        Debug.Log("function that a user did not create has been called. : listRmiStub.Count > 0");
                    }
                }
            }
        }


        /*
         * ��Ŷ����
         * ť�� ��� ���� ��쿡�� ť�� �߰��� �� �ٷ� Send �ϰ�
         * �����Ͱ� ��� ���� ��쿡�� ���� �߰��� �Ѵ�.
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
         * Ŭ���̾�Ʈ Tick ���� ȣ�����־�� �Ѵ�.
         * Recv �� ��Ŷ�� queue �Ǿ� ���� ��� 1 frame �� 1���� packet �� OnPacketReceive() �� ���� ���� ���ش�
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

                    // peek => Dequeue�������� ���� ���� ���������� Queue���� ���������� �ʴ´�.
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
         * Socket.BeginSend() �� ���ε� �Ǿ� Send �Ϸ�� ȣ��
         * P.S : host thread ���� ȣ���
         */
        private void OnCallbackSend(IAsyncResult ar)
        {
            try
            {
                Socket socketTemp = (Socket)ar.AsyncState;

                int sendSize = socketTemp.EndSend(ar);

                Debug.LogFormat("���������� ���� : {0}", sendSize);               

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

                        Debug.LogFormat("���� ������: {0}", packet.jsonBuffer.ToString(Newtonsoft.Json.Formatting.Indented));
                        //Debug.LogFormat("���� ������: {0}", Encoding.UTF8.GetString(packet.buffer));
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
         * Socket.BeginReceive() ���ε� �Ǿ� Packet �� �ް� �Ǹ� ȣ��
         * P.S : host thread ���� ȣ���
         */
        private void OnCallbackReceive(IAsyncResult ar)
        {
            try
            {
                Socket socketTemp = (Socket)ar.AsyncState;

                if (socketTemp == null)
                    return;

                int readSize = socketTemp.EndReceive(ar);

                Debug.LogFormat("���� ������ ����: {0}", readSize);

                //Debug.Log(Encoding.UTF8.GetString(bufferRecv, BasicType.HEADSIZE, readSize));

                if (readSize > 0)
                {
                    buffRecvSize += readSize;

                    /*
                    ��Ŷ�� ���������� ���ų�
                    2�� �̻��� ��Ŷ�� �ϳ��� �ü� �־ üũ�Ѵ�(���̱� �˰���)
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

                            // ��ȣȭ�ߴٸ� �� ���̿��� ��ȣȭ
                            msgRecv.ReadEnd();
                            Debug.LogFormat("���� ������: {0}", msgRecv.jsonBuffer.ToString(Newtonsoft.Json.Formatting.Indented));

                            lock (csReceive)
                            {
                                // Ŭ�� ������� ����
                                queueReceive.Enqueue(msgRecv);
                            }
                        }   
                        else
                        {
                            // ������ ��Ŷ�� �����ϱ⿡ ����� �����Ͱ� ����. loop�� ����������.
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
                //Debug.LogFormat("���� ������: {0}", data);

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



    // �̰� ������������ �����ؿԴµ� ���� ��������
    /// <summary>
    ///
    /// ���� ���ӽ� ����� �Ķ����
    /// ����
    ///      hostname : ������ ������ IP �Ǵ� ȣ��Ʈ����
    ///      port : ������ ��Ʈ
    ///      OnJoinServerComplete : ���� ������ ȣ�� �� �Լ� ������   
    ///      OnLeaveServer : ���� ���� ����� ȣ�� �� �Լ� ������
    ///      OnReceiveMessage : Packet receive �� �޴� �Լ�(NIDL �� ������� �ʰ� ���� ó�� �� �� ��� �ϸ��)
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
