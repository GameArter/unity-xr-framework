using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(HandController))]

public class HandControllerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("controllerNode"), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("controller"), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("hand"), true);
		EditorGUILayout.EndHorizontal();

		// Save values
		serializedObject.ApplyModifiedProperties();
	}
}
