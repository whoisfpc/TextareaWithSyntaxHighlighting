using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ExampleWindow : EditorWindow
{
    [MenuItem("Window/ExampleWindow")]
    static void Open()
    {
        GetWindow<ExampleWindow>(nameof(ExampleWindow));
    }

    private string text;
    private int fontSize = 20;
    private bool syntaxColor = true;
    private bool richText = true;

    private ColorfulTextarea colorfulTextarea; 

    void Awake()
    {
        colorfulTextarea = new ColorfulTextarea();
        colorfulTextarea.Awake();
    }

    void OnDestroy()
    {
        colorfulTextarea.OnDestroy();
    }

    void OnGUI()
    {
        using(var h = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Font Size", GUILayout.Width(70));
            fontSize = EditorGUILayout.IntSlider(fontSize, 12, 30);
            GUILayout.FlexibleSpace();
            syntaxColor = GUILayout.Toggle(syntaxColor, "Syntax Color");
            richText = GUILayout.Toggle(richText, "richText");
        }
        colorfulTextarea.fontSize = fontSize;
        colorfulTextarea.syntaxColor = syntaxColor;
        colorfulTextarea.richText = richText;
        text = colorfulTextarea.Draw(GUILayout.ExpandHeight(true));
    }

}
