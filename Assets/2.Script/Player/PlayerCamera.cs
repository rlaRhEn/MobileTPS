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
            // 내 카메라 활성화
       
            camera.enabled = true;

        }
        else
        {
            // 다른 유저 카메라는 끄고 태그 제거
            //var camera = transform.Find("CameraArm/Main Camera").GetComponent<Camera>();
            camera.enabled = false;
            camera.tag = "Untagged";
        }
    }
}
