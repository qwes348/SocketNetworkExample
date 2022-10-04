using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGNet;

public class CustomMessageMarshal : NGNet.MessageMarshal
{
    #region Vector3
    public static void Read(Message msg, out Vec3 v)
    {
        v = new Vec3();
        Read(msg, out v.x);
        Read(msg, out v.y);
        Read(msg, out v.z);
    }

    public static void Write(Message msg, Vec3 v)
    {
        Write(msg, v.x);
        Write(msg, v.y);
        Write(msg, v.z);
    }
    #endregion

    #region Quaternion
    public static void Read(Message msg, out Quater4 q)
    {
        q = new Quater4();
        Read(msg, out q.x);
        Read(msg, out q.y);
        Read(msg, out q.z);
        Read(msg, out q.w);
    }

    public static void Write(Message msg, Quater4 q)
    {
        Write(msg, q.x);
        Write(msg, q.y);
        Write(msg, q.z);
        Write(msg, q.w);
    }
    #endregion
}
