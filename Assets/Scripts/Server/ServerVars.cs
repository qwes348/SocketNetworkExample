using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * 서버 패킷에 실어보내는 커스텀 클래스형들 정의하는곳
 * 클래스를 분해해서 실어서 보내고 읽어올때도 분해된 데이터들을
 * 클래스로 조립해서 반환받기 위함
 * 
 * CustomMessageMarshal 클래스에서 사용
 */

[Serializable]
public class Vec3
{
    public float x;
    public float y;
    public float z;

    public Vec3()
    {

    }

    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vec3(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class Quater4
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Quater4()
    {

    }

    public Quater4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Quater4(Quaternion rot)
    {
        x = rot.x;
        y = rot.y;
        z = rot.z;
        w = rot.w;
    }

    public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
}
