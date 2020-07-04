using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Translate))]


public class TranslateEditor : Editor
{
    string newKey = "";
    string newValue = "";
    bool showKeys = true;
    bool showValues = true;

    //int index = 0;
    //List<string> options = new List<string>();

    public override void OnInspectorGUI()
    {


        Language lang = Resources.Load("Configuration/Language/Polish") as Language;
        Translate translate = (Translate)target;
        EditorGUILayout.LabelField("Key: ", translate.key);
        EditorGUILayout.LabelField("Value: ", lang.words.Find(word => word.key == translate.key).value);

        newKey = EditorGUILayout.TextField("Find By Key", newKey);
        newValue = EditorGUILayout.TextField("Find By Value", newValue);

        bool WordsContainsValue(string value)
        {
            foreach (Word word in lang.words)
            {
                if (word.value.ToLower().Contains(value.ToLower()) && value != "")
                {
                    return true;
                }
            }
            return false;
        }

        bool WordsContainsKey(string key)
        {
            foreach (Word word in lang.words)
            {
                if (word.key.ToLower().Contains(key.ToLower()) && key != "")
                {
                    return true;
                }
            }
            return false;
        }

        showKeys = (WordsContainsKey(newKey)) ? EditorGUILayout.BeginFoldoutHeaderGroup(showKeys, "Found Keys") : true;
        if (showKeys)
        {
            for (int i = 0; i < lang.words.Count; i++)
            {
                if (lang.words[i].key.ToLower().Contains(newKey.ToLower()) && newKey != "")
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(lang.words[i].key, lang.words[i].value);
                    if (GUILayout.Button("SET"))
                    {
                        translate.key = lang.words[i].key;
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        showValues = (WordsContainsValue(newValue)) ? EditorGUILayout.BeginFoldoutHeaderGroup(showValues, "Found Values") : true;
        if (showValues)
        {
            for (int i = 0; i < lang.words.Count; i++)
            {
                if (lang.words[i].value.ToLower().Contains(newValue.ToLower()) && newValue != "")
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(lang.words[i].key, lang.words[i].value);
                    if (GUILayout.Button("SET"))
                    {
                        translate.key = lang.words[i].key;
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
