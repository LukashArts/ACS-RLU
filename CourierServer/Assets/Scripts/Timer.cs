using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI text;
    public static float timeRemaining = 185;

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime / 2;
            if (text != null)
            {
                text.text = timeRemaining.ToString("##");
                ServerSend.TimeRemaining(timeRemaining);
            }
        }
    }
}
