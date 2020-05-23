using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LanguageController
{
    public List<Language> languages = new List<Language>();



    public Language GetLang() 
    {
        foreach (Language lang in languages)
        {
            if (lang.language == SettingsController.instance.settings.language)
            {
                return lang;
            }
        }
        return null;
    }
    public string GetWord(string key)
    {
        foreach (Word w in GetLang().words)
        {
            if (w.key == key) return w.value;
        }
        //dodac lambda expression do szukania
        return "";
    }
}
