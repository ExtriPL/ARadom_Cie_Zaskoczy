using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPressed : MonoBehaviour
{
    private Button button { get => GetComponent<Button>(); }
    private AudioSource source { get => SettingsController.instance.buttonAudioSource; }

    private void Start()
    {
        button?.onClick.AddListener(() => PlaySound());
    }

    void PlaySound()
    {
        source.Play();
    }
}
