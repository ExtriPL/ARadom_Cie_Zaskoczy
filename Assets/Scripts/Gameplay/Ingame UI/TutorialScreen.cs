using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    [HideInInspector]
    public int timer;
    public GameObject closeButton;
    public TextMeshProUGUI timerText;

    private void Update()
    {
        timerText.text = timer == 0 ? "" : timer.ToString();
    }
}
