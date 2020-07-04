using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Language))]
public class LanguageEditor : Editor
{
    string newKey = "";
    string newValue = "";
    bool showWords = false;
    bool showKeys = true;
    bool showValues = true;
    public override void OnInspectorGUI()
    {
        Language lang = (Language)target;
        base.OnInspectorGUI();
        
        newKey = EditorGUILayout.TextField("New Key", newKey);
        newValue = EditorGUILayout.TextField("New Value", newValue);       

        EditorGUILayout.LabelField("Word count: ", lang.words.Count.ToString());
   
        if (lang.words.Contains(lang.words.Find(word => word.key == newKey)))
        {
            string value = lang.words.Find(word => word.key == newKey).value;
            EditorGUILayout.LabelField("Key Aready Exists, Value: ", value);
            
        }
        else if (lang.words.Contains(lang.words.Find(word => word.value == newValue)))
        {
            string key = lang.words.Find(word => word.value == newValue).key;
            EditorGUILayout.LabelField("Value Aready Exists, Key: ", key);
        }


        else if(newKey != "")
        {
            if (GUILayout.Button("ADD")) 
            {
                lang.words.Add(new Word(newKey, newValue));
                newKey = "";
                newValue = "";
                showWords = true;
                EditorUtility.SetDirty(target);
            }
        }

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
                    if (GUILayout.Button("DELETE"))
                    {
                        lang.words.Remove(lang.words[i]);
                        EditorUtility.SetDirty(target);
                    }
                    if (GUILayout.Button("MODIFY"))
                    {
                        newKey = lang.words[i].key;
                        newValue = lang.words[i].value;
                        lang.words.Remove(lang.words[i]);
                        showWords = false;
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
                    if (GUILayout.Button("DELETE"))
                    {
                        lang.words.Remove(lang.words[i]);
                        EditorUtility.SetDirty(target);
                    }
                    if (GUILayout.Button("MODIFY"))
                    {
                        newKey = lang.words[i].key;
                        newValue = lang.words[i].value;
                        lang.words.Remove(lang.words[i]);
                        showWords = false;
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        

        showWords = EditorGUILayout.BeginFoldoutHeaderGroup(showWords, "Words");
        if (showWords)
        {
            for (int i = 0; i < lang.words.Count; i++) 
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(lang.words[i].key, lang.words[i].value);
                if (GUILayout.Button("DELETE")) 
                {
                    lang.words.Remove(lang.words[i]);
                    EditorUtility.SetDirty(target);
                }
                if (GUILayout.Button("MODIFY"))
                { 
                    newKey = lang.words[i].key;
                    newValue = lang.words[i].value;
                    lang.words.Remove(lang.words[i]);
                    showWords = false;
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
