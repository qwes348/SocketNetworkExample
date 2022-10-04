using System;
using UnityEngine;
using NaughtyAttributes;

public class Poolable : MonoBehaviour
{
    public string id;
    public int createCountInAwake = 0;
    public bool isUsing = false;
    public bool isAutoPooling;
    [ShowIf("isAutoPooling")]
    public float autoPoolingTime;

    public Action onPop;
    public Action onPush;

    private float poolingTimer = 0f;

    private void Update()
    {
        if(isAutoPooling && isUsing)
        {
            if(poolingTimer < autoPoolingTime)
                poolingTimer += Time.deltaTime;
            else
            {
                poolingTimer = 0f;
                PoolManager.instance.Push(this);                
            }
        }
    }

    // 원래는 Vfx 타겟에 붙일용도로 생각했지만 로봇이 비활성화되면 다음 라운드가 돼서 로봇이 활성화 되기전까진 돌아올수없음 그것도 그것대로 문제인것같아서 안씀
    public void ChangeParent(Transform newParent, bool resetParentOnPush = true)
    {
        transform.parent = newParent;        

        if(resetParentOnPush)
        {
            onPush += () => transform.parent = PoolManager.instance.transform;
        }
    }

    public void ResetAutoPoolingTimer()
    {
        poolingTimer = 0f;
    }
}
