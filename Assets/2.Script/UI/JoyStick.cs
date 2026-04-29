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
