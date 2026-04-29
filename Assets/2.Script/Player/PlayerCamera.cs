using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviourPun
{
    private void Start()
    {
        FindCharacter();
    }
    void FindCharacter()
    {
            var camera = transform.Find("CameraArm/Main Camera").GetComponent<Camera>();
            if (photonView.IsMine)
            {
                // 내 카메라 활성화
                camera.enabled = true;
            }
            else
            {
                camera.enabled = false;
                camera.tag = "Untagged";
            }
        
    }
}
