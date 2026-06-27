using UnityEngine;
using UnityEditor;

public class ViewOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ViewOnlyAttribute))]
public class ViewOnlyPropertyDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    GUI.enabled = false;
    EditorGUI.PropertyField(position, property, label, true);
    GUI.enabled = true;
  }
}
#endif