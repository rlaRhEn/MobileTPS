using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class KillLogo : MonoBehaviour
{
    public Text killerText;
    public Text victimText;
    
    public void SetUp(string killer, string victim)
    {
        if(killerText != null)
        {
            killerText.text = killer;
        }
        if(victimText != null)
        {
            victimText.text = victim;
        }   
    }
}
