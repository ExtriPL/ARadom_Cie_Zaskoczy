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
            if (w.key.ToLower() == key.ToLower()) return w.value;
        }
        //dodac lambda expression do szukania
        return key;
    }

    public string GetKey(string value) 
    {
        foreach (Word w in GetLang().words)
        {
            if (w.value.ToLower() == value.ToLower()) return w.key;
        }
        //dodac lambda expression do szukania
        return "Wartość: " + value +" nie istnieje!";
    }

    public string PackKey(string value)
    {
        return "{t}" + value;
    }

    public bool IsPacked(string value)
    {
        return value.Length >= 3 && value.Substring(0, 3).Equals("{t}");
    }

    public string UnpackKey(string value)
    {
        return value.Replace("{t}", "");
    }
}
