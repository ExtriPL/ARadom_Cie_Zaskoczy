using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public SoundEffectType effectType;
    protected SoundController sound { get => SettingsController.instance.soundController; }

    public virtual void PlayEffect()
    {
        sound.PlayEffect(effectType);
    }
}