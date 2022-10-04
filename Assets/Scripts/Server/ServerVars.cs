using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * ���� ��Ŷ�� �Ǿ���� Ŀ���� Ŭ�������� �����ϴ°�
 * Ŭ������ �����ؼ� �Ǿ ������ �о�ö��� ���ص� �����͵���
 * Ŭ������ �����ؼ� ��ȯ�ޱ� ����
 * 
 * CustomMessageMarshal Ŭ�������� ���
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
