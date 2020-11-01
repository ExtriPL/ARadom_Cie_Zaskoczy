using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientSoundPlayer : SoundPlayer
{
    public AudioSource source;
    public override void PlayEffect()
    {
        source.clip = sound.GetSoundEffect(effectType);
        source.volume = SettingsController.instance.settings.soundEffectsVolume / Keys.Menu.MAX_MUSIC_VOLUME;
        source.Play();
    }
}