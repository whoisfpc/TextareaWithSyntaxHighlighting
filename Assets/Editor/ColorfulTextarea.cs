using UnityEngine;
using UnityEditor;


[System.Serializable]
public class ColorfulTextarea
{
    public class TextContainer : ScriptableObject
    {
        public string text;
    }

    private const string DEFAULT_FONT = "Consolas";
    private const string CONTROL_NAME = "ColorfulTextarea";
    public bool syntaxColor = true;

    public int fontSize = 20;

    private Font codeFont;

    private GUIStyle backStyle;
    private GUIStyle frontStyle;

    private TextContainer textContainer;

    private string cachedText;
    private string cachedColorText;

    // 用于undo系统记录文本是否改变
    private bool textChangedForUndo = false;
    private bool forceIncrementCurrentGroup = false;

    public void Awake()
    {
        codeFont = Font.CreateDynamicFontFromOSFont(DEFAULT_FONT, fontSize);
        textContainer = ScriptableObject.CreateInstance<TextContainer>();
        textContainer.hideFlags = HideFlags.DontSave;
    }

    public void OnDestroy()
    {
        Object.DestroyImmediate(codeFont);
        codeFont = null;
        Object.DestroyImmediate(textContainer);
        textContainer = null;
    }

    private void Highlighter()
    {
        if (cachedText != textContainer.text)
        {
            cachedColorText = cachedText = textContainer.text;
            cachedColorText = $"<color=red>{cachedColorText}</color>";
        }
    }

    public string Draw(params GUILayoutOption[] options)
    {
        if (backStyle == null)
        {
            backStyle = new GUIStyle(EditorStyles.textArea);
            backStyle.font = codeFont;
            backStyle.wordWrap = false;
            frontStyle = new GUIStyle(backStyle);
            frontStyle.richText = true;
        }
        
        if (codeFont == null)
        {
            codeFont = Font.CreateDynamicFontFromOSFont(DEFAULT_FONT, fontSize);
            backStyle.font = codeFont;
            frontStyle.font = codeFont;
        }

        backStyle.fontSize = fontSize;
        frontStyle.fontSize = fontSize;

        if (GUI.GetNameOfFocusedControl() == CONTROL_NAME)
        {
            Event ev = Event.current;
            switch (ev.type)
            {
                case EventType.KeyDown:
                {
                    if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    {
                        if (textChangedForUndo)
                        {
                            textChangedForUndo = false;
                            forceIncrementCurrentGroup = true;
                        }
                    }
                    break;
                }
            }
        }

        var oldContentColor = GUI.contentColor;
        if (syntaxColor)
        {
            GUI.contentColor = Color.clear;
        }

        GUI.SetNextControlName(CONTROL_NAME);
        EditorGUI.BeginChangeCheck();
        var text = EditorGUILayout.TextArea(textContainer.text, backStyle, options);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(textContainer, "ColorfulTextarea");
            textContainer.text = text;
            textChangedForUndo = true;
            if (forceIncrementCurrentGroup)
            {
                Undo.IncrementCurrentGroup();
                forceIncrementCurrentGroup = false;
            }
        }

        GUI.contentColor = oldContentColor;

        if (syntaxColor && Event.current.type == EventType.Repaint)
        {
            var oldBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.clear;
            Highlighter();
            frontStyle.Draw(GUILayoutUtility.GetLastRect(), new GUIContent(cachedColorText), 0);
            GUI.backgroundColor = oldBackgroundColor;
        }

        return text;
    }
}
