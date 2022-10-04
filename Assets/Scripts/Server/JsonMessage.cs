using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NGNet;
using System;
using System.Text;

/* 서버에 byte데이터 대신 Json으로
 * 보내기로 결정함
 * Json데이터를 보내기 위한 데이터 버퍼
 */

namespace AIGears.Server
{
    public class JsonMessage
    {
        public byte[] buffer = null;
        public JObject jsonBuffer;
        private JObject Data { get; set; }

        public enum TargetEnum { None = -1, MySelf, AllPlayersInTheRoom, OtherPlayersInTheRoom, AllPlayers, OtherPlayers }

        // 패킷의 이벤트ID (어떤 리퀘스트인지 구분용)
        public int ID
        {
            get
            {
                if (jsonBuffer.TryGetValue("EventID", out JToken idToken))
                {
                    return int.Parse(idToken.ToString());
                }
                else
                    return -1;
            }
            set
            {
                jsonBuffer["EventID"] = value;
            }
        }

        // 클라이언트의 고유한 ID
        public string ClientID
        {
            get
            {
                if (jsonBuffer.TryGetValue("ClientID", out JToken idToken))
                {
                    return idToken.ToString();
                }
                else
                    return string.Empty;
            }
            set
            {
                jsonBuffer["ClientID"] = value;
            }
        }

        // 패킷의 길이
        public int Length
        {
            get
            {
                //if (jsonBuffer.TryGetValue("Length", out JToken token))
                //{
                //    if (int.TryParse(token.ToString(), out int result))
                //    {
                //        return result;
                //    }
                //}

                //return -1;

                //return jsonBuffer.ToString(Formatting.None).Length;

                //return BitConverter.ToInt32(buffer, 0);

                if (jsonBuffer.TryGetValue("data", out JToken token))
                    return Encoding.UTF8.GetByteCount(jsonBuffer.ToString(Formatting.None)) + sizeof(int);
                else
                    return Encoding.UTF8.GetByteCount(jsonBuffer.ToString(Formatting.None)) + 
                        Encoding.UTF8.GetByteCount(Data.ToString(Formatting.None)) + 
                        sizeof(int);
            }
            //set
            //{
            //    byte[] ln = BitConverter.GetBytes(value + sizeof(int));
            //    ln.CopyTo(buffer, BasicType.HEAD_POS_PACKET_SIZE);
            //}
        }

        // 패킷을 전달받을 타겟
        public TargetEnum Target
        {
            get
            {
                if(jsonBuffer.TryGetValue("Target", out JToken token))
                {
                    if(int.TryParse(token.ToString(), out int result))
                    {
                        return (TargetEnum)result;
                    }
                }

                return TargetEnum.None;
            }
            set
            {
                jsonBuffer["Target"] = (int)value;
            }
        }

        public JsonMessage()
        {
            jsonBuffer = new JObject();
            Data = new JObject();
            buffer = new byte[BasicType.MAX_PACKET_SIZE];
            Array.Clear(buffer, 0, BasicType.MAX_PACKET_SIZE);
        }

        public string ConvertToString()
        {
            return jsonBuffer.ToString();
        }

        public void WriteEnd()
        {
            jsonBuffer.Add("data", Data);

            byte[] ln = BitConverter.GetBytes(Length);
            ln.CopyTo(buffer, BasicType.HEAD_POS_PACKET_SIZE);

            byte[] value = Encoding.UTF8.GetBytes(jsonBuffer.ToString(Formatting.None));     
            value.CopyTo(buffer, BasicType.HEADSIZE);
        }

        public void ReadEnd()
        {
            int len = BitConverter.ToInt32(buffer, 0);
            string json = Encoding.UTF8.GetString(buffer, 4, len - BasicType.HEADSIZE);
            jsonBuffer = JObject.Parse(json);
            Data = JObject.Parse(jsonBuffer["data"].ToString());
        }

        #region 데이터 쓰기
        
        /*
         * 기본형 Type
         */
        public void Write(string key, int value)
        {
            Data[key] = value;
        }

        public void Write(string key, float value)
        {
            if (value % 1 == 0)
                Data[key] = (int)value;
            else
                Data[key] = value;
        }

        public void Write(string key, string value)
        {
            Data[key] = value;
        }

        public void Write(string key, bool value)
        {
            Data[key] = value;
        }

        /*
         * 커스텀 Type
         */
        public void Write(string key, Vector3 value)
        {
            Write(key + "_x", value.x);
            Write(key + "_y", value.y);
            Write(key + "_z", value.z);
        }

        public void Write(string key, Quaternion value)
        {
            Write(key + "_x", value.x);
            Write(key + "_y", value.y);
            Write(key + "_z", value.z);
            Write(key + "_w", value.w);
        }

        #endregion


        #region 데이터 읽기
        /*
         * 기본Type
         */
        public void Read(string key, out int result)
        {
            Data.TryGetValue(key, out JToken token);
            string value = token.ToString();
            int.TryParse(value, out result);
        }

        public void Read(string key, out float result)
        {
            Data.TryGetValue(key, out JToken token);
            string value = token.ToString();
            float.TryParse(value, out result);
        }

        public void Read(string key, out string result)
        {
            if (Data.TryGetValue(key, out JToken token))
                result = token.ToString();
            else
                result = string.Empty;
        }

        public void Read(string key, out bool result)
        {
            Data.TryGetValue(key, out JToken token);
            string value = token.ToString();
            result = value == "true";
        }

        /*
         *  커스텀Type
         */
        public void Read(string key, out Vector3 result)
        {

            Read(key + "_x", out float x);
            Read(key + "_y", out float y);
            Read(key + "_z", out float z);

            result = new Vector3(x, y, z);
        }

        public void Read(string key, out Quaternion result)
        {

            Read(key + "_x", out float x);
            Read(key + "_y", out float y);
            Read(key + "_z", out float z);
            Read(key + "_w", out float w);

            result = new Quaternion(x, y, z, w);
        }
        #endregion
    }
}

