#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    private Type[] effectTypes;
    private string[] effectTypeNames;

    private void OnEnable()
    {
        // Find all types that inherit from ItemEffect
        effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(ItemEffect)) && !type.IsAbstract)
            .ToArray();

        effectTypeNames = effectTypes.Select(t => t.Name).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        ItemData itemData = (ItemData)target;

        // Display existing effects
        for (int i = 0; i < itemData.effects.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (itemData.effects[i] != null)
            {
                EditorGUILayout.LabelField(itemData.effects[i].GetType().Name, EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Null Effect", EditorStyles.boldLabel);
            }

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                itemData.effects.RemoveAt(i);
                EditorUtility.SetDirty(target);
                serializedObject.Update();
                break;
            }

            EditorGUILayout.EndHorizontal();

            // Draw the effect's fields
            if (itemData.effects[i] != null)
            {
                SerializedProperty effectsProp = serializedObject.FindProperty("effects");
                SerializedProperty effectProp = effectsProp.GetArrayElementAtIndex(i);

                // Use a different approach to iterate through fields
                EditorGUI.indentLevel++;

                // Get the iterator and make a copy for the end property
                SerializedProperty iterator = effectProp.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();

                // Enter children and iterate through all properties
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false; // Only enter children on first iteration
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add new effect button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Effect"))
        {
            ShowEffectSelectionMenu(itemData);
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowEffectSelectionMenu(ItemData itemData)
    {
        GenericMenu menu = new GenericMenu();

        foreach (Type effectType in effectTypes)
        {
            menu.AddItem(new GUIContent(effectType.Name), false, () =>
            {
                ItemEffect newEffect = (ItemEffect)Activator.CreateInstance(effectType);
                itemData.effects.Add(newEffect);
                EditorUtility.SetDirty(target);
                serializedObject.Update();
            });
        }

        menu.ShowAsContext();
    }
}
#endif