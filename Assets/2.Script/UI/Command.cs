using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

public enum CommandKey { jump, shot, dodge, reload }

public class Command : MonoBehaviourPun, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    PlayerMove playerMove;



    Button button;

    bool isHolding;

    public CommandKey command;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    void Update()
    {
        if(command == CommandKey.shot && isHolding)
        {
            playerMove.Fire();
        }
        if (command == CommandKey.dodge && isHolding)
        {
            playerMove.Dodge();
        }
    }
    public void SetPlayerKey(PlayerMove player)
    {
        playerMove = player;

        switch (command)
        {
            case CommandKey.shot:
                
                break;
            case CommandKey.dodge:
                //button.onClick.AddListener(playerMove.Dodge);
                break;
            case CommandKey.jump:
                button.onClick.AddListener(playerMove.Jump);
                break;
            case CommandKey.reload:
                button.onClick.AddListener(playerMove.Reload);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (command == CommandKey.shot)
        {
            isHolding = false;
        }
        if (command == CommandKey.dodge)
        {
            isHolding = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(command == CommandKey.shot)
        {
            isHolding = true;
        }
        if (command == CommandKey.dodge)
        {
            isHolding = true;
        }
    }
}
