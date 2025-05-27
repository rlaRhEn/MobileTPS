using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class CameraLookDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    bool isDragging;
    Vector2 previousPos;

    [SerializeField]
    Transform cameraArm;

    public void SetCameraArm(Transform arm)
    {
        cameraArm = arm;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cameraArm == null || !cameraArm.root.GetComponent<PhotonView>().IsMine) return;

        isDragging = true;
        previousPos = eventData.position;   
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragging) return;
        if (cameraArm == null || !cameraArm.root.GetComponent<PhotonView>().IsMine) return;

        Vector2 delta = eventData.position - previousPos;
        previousPos = eventData.position;

        LookAround(delta * 0.2f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
    public void LookAround(Vector2 inputDir) //ī�޶� ȸ���� ���� ��������ϱ�
    {
        if (cameraArm == null || !cameraArm.root.GetComponent<PhotonView>().IsMine) return;

        Vector3 camAngle = cameraArm.rotation.eulerAngles;
        float x = camAngle.x - inputDir.y;

        if (x < 180f) //���� ��������
        {
            x = Mathf.Clamp(x, -1f, 25f);
        }
        else //�Ʒ��� ��������
        {
            x = Mathf.Clamp(x, 352f, 361f);
        }
        cameraArm.rotation = Quaternion.Euler(x, camAngle.y + inputDir.x, camAngle.z);

    }
}
