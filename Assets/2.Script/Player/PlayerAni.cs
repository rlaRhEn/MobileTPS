using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAni : MonoBehaviour
{
    Animator ani;

    private void Awake()
    {
        ani = GetComponentInChildren<Animator>(); //�ڽ� ������Ʈ ��������
    }

    public void TriggerAni(string aniName)
    {
        ani.SetTrigger(aniName);
    }
    public void RunAni(bool isAni)
    {
        ani.SetBool("IsRun", isAni);
    }
    
}
