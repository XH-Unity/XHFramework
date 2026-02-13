using UnityEngine;
using UnityEditor;

namespace XHFramework.Editor {

/// <summary>
/// 工具面板基类 - 定义所有工具面板的基本接口
/// </summary>
public abstract class BaseToolPanel
{
    public abstract string PanelName { get; }
    public abstract string PanelIcon { get; }
    public abstract string Description { get; }

    /// <summary>
    /// 绘制面板内容
    /// </summary>
    public abstract void OnGUI();

    /// <summary>
    /// 面板激活时调用
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// 面板失活时调用
    /// </summary>
    public virtual void OnDisable() { }

    /// <summary>
    /// 面板销毁时调用
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// 绘制标题栏
    /// </summary>
    protected void DrawHeader()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(PanelIcon, GUILayout.Width(24), GUILayout.Height(24));
        GUILayout.Space(8);
        GUILayout.Label(PanelName, EditorStyles.largeLabel);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
    }

    /// <summary>
    /// 绘制分组标题
    /// </summary>
    protected bool DrawFoldoutGroup(string title, bool foldout, System.Action content)
    {
        GUILayout.Space(5);
        bool newFoldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);

        if (newFoldout)
        {
            EditorGUI.indentLevel++;
            content?.Invoke();
            EditorGUI.indentLevel--;
        }

        return newFoldout;
    }

    /// <summary>
    /// 绘制操作按钮组
    /// </summary>
    protected void DrawButtonGroup(string title, string description, params ButtonInfo[] buttons)
    {
        EditorGUILayout.BeginVertical("box");

        if (!string.IsNullOrEmpty(title))
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
        }

        if (!string.IsNullOrEmpty(description))
        {
            EditorGUILayout.HelpBox(description, MessageType.None);
        }

        // 根据按钮数量自动排列
        if (buttons.Length <= 2)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var btn in buttons)
            {
                DrawButton(btn);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            for (int i = 0; i < buttons.Length; i += 2)
            {
                EditorGUILayout.BeginHorizontal();
                DrawButton(buttons[i]);
                if (i + 1 < buttons.Length)
                {
                    DrawButton(buttons[i + 1]);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制单个按钮
    /// </summary>
    private void DrawButton(ButtonInfo buttonInfo)
    {
        // 确保在OnGUI上下文中创建样式
        var style = new GUIStyle(GUI.skin.button);

        if (buttonInfo.IsWarning)
        {
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);

            if (GUILayout.Button(buttonInfo.Text, style, GUILayout.Height(buttonInfo.Height)))
            {
                if (string.IsNullOrEmpty(buttonInfo.ConfirmMessage) ||
                    EditorUtility.DisplayDialog("确认操作", buttonInfo.ConfirmMessage, "确认", "取消"))
                {
                    buttonInfo.Action?.Invoke();
                }
            }

            GUI.backgroundColor = originalColor;
        }
        else
        {
            if (GUILayout.Button(buttonInfo.Text, style, GUILayout.Height(buttonInfo.Height)))
            {
                if (!string.IsNullOrEmpty(buttonInfo.ConfirmMessage))
                {
                    if (EditorUtility.DisplayDialog("确认操作", buttonInfo.ConfirmMessage, "确认", "取消"))
                    {
                        buttonInfo.Action?.Invoke();
                    }
                }
                else
                {
                    buttonInfo.Action?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// 按钮信息结构体
    /// </summary>
    protected struct ButtonInfo
    {
        public string Text;
        public System.Action Action;
        public string ConfirmMessage;
        public bool IsWarning;
        public int Height;

        public ButtonInfo(string text, System.Action action, string confirmMessage = null, bool isWarning = false, int height = 30)
        {
            Text = text;
            Action = action;
            ConfirmMessage = confirmMessage;
            IsWarning = isWarning;
            Height = height;
        }
    }
}

}