using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Ustawienia aplikacji. Służy do serializacji
/// </summary>
[System.Serializable]
public struct ApplicationSettings
{
    public float musicVolume;
    public float soundEffectsVolume;
    public Languages language;
}
