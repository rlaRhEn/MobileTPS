using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviourPun
{
    private void Start()
    {
        var camera = transform.Find("CameraArm/Main Camera").GetComponent<Camera>();
        if (photonView.IsMine)
        {
            // �� ī�޶� Ȱ��ȭ
       
            camera.enabled = true;

        }
        else
        {
            // �ٸ� ���� ī�޶�� ���� �±� ����
            //var camera = transform.Find("CameraArm/Main Camera").GetComponent<Camera>();
            camera.enabled = false;
            camera.tag = "Untagged";
        }
    }
}
