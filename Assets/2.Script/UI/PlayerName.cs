using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    private Text nameText;

    private void Awake()
    {
        nameText = GetComponent<Text>();

    }
    private void Start()
    {
        if(AuthManager.user != null)
        {
            nameText.text = $"Hi! {AuthManager.user.Email}";
        }
        else { nameText.text = "Login Error"; }
    }
}
