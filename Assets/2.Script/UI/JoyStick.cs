using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour,  IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    RectTransform lever;
    RectTransform rectTransform;


    Vector2 inputDirection;
   
    [SerializeField, Range(10,150)]
    float leverRange;

    [SerializeField]
    PlayerMove playerMove;

    bool isInput;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetPlayerKey(PlayerMove player)
    {
        playerMove = player;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

        ControlJoyStickLever(eventData);
        isInput = true;

    }
    //왜 드래그로안하고 update문에서 호출하는지?
    //오브젝트를 클릭해서 드래그하는 도중에 들어오는 이벤트
    //하지만 클릭을 유지한 상태로 마우스를 멈추면 이벤트가 들어오지않음
    public void OnDrag(PointerEventData eventData)
    {
        ControlJoyStickLever(eventData);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector3.zero;
        isInput = false;
        playerMove.Move(Vector2.zero);
    }

    void ControlJoyStickLever(PointerEventData eventData)
    {
        var inputPos = eventData.position - rectTransform.anchoredPosition;
        var inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        lever.anchoredPosition = inputVector;
        inputDirection = inputVector / leverRange;
    }

    void InputControlVector()
    {
        playerMove.Move(inputDirection);
    }

    // Update is called once per frame
    void Update()
    {
       if(playerMove != null && playerMove.photonView.IsMine)
        {
            if (isInput)
            {
                InputControlVector();
            }
        }
       
    }
}
