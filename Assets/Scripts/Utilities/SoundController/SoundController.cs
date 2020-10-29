using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField]
    private AudioSource musicSource, soundEffectSource;
    [SerializeField]
    private List<AudioClip> musics = new List<AudioClip>();
    [SerializeField]
    private List<AudioClip> effects = new List<AudioClip>();
    private int currentMusic = 0;

    private void Start()
    {
        NextMusic();
    }

    private void Update()
    {
        if(!musicSource.isPlaying)
        {
            NextMusic();
        }
    }

    private void NextMusic()
    {
        currentMusic++;
        if(currentMusic==musics.Count)
        {
            currentMusic = 0;
        }
        musicSource.clip = musics[currentMusic];
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume / Keys.Menu.MAX_MUSIC_VOLUME;
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectSource.volume = volume / Keys.Menu.MAX_MUSIC_VOLUME;
    }

    public AudioClip GetSoundEffect(SoundEffectType type)
    {
        return effects[(int)type];
    }

    public void PlayEffect(SoundEffectType type)
    {
        soundEffectSource.clip = GetSoundEffect(type);
        soundEffectSource.Play();
    }
}

public enum SoundEffectType
{ 
    ButtonClick,
    BuildingSound,
    PopupSound1,
    PopupSound2,
    PopupSound3,
    PopupSound4
}
