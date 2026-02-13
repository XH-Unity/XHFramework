using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace XHFramework.Editor {

/// <summary>
/// 配置面板 - 宏定义配置部分
/// </summary>
public partial class ConfigPanel
{
    // 宏定义字段
    private string _macroDefinesInput = "";
    private List<string> _currentMacroDefines = new List<string>();
    private bool _showPresetMacros = false;
    private Vector2 _presetMacrosScrollPos;

    /// <summary>
    /// 预设宏定义列表（按分类组织）
    /// </summary>
    private static readonly Dictionary<string, List<MacroDefineInfo>> PresetMacroCategories = new Dictionary<string, List<MacroDefineInfo>>
    {
        {
            "调试相关", new List<MacroDefineInfo>
            {
                new MacroDefineInfo("DEBUG", "调试模式，启用调试相关代码"),
                new MacroDefineInfo("ENABLE_LOG", "启用日志输出"),
                new MacroDefineInfo("ENABLE_PROFILER", "启用性能分析器"),
                new MacroDefineInfo("ENABLE_CONSOLE", "启用控制台"),
            }
        },
        {
            "功能开关", new List<MacroDefineInfo>
            {
                new MacroDefineInfo("ENABLE_GM", "启用 GM 命令"),
                new MacroDefineInfo("ENABLE_GUIDE", "启用新手引导"),
                new MacroDefineInfo("ENABLE_HOTFIX", "启用热更新"),
                new MacroDefineInfo("ENABLE_SDK", "启用 SDK 功能"),
                new MacroDefineInfo("ENABLE_ANALYTICS", "启用数据统计"),
            }
        },
        {
            "网络相关", new List<MacroDefineInfo>
            {
                new MacroDefineInfo("USE_TCP", "使用 TCP 网络"),
                new MacroDefineInfo("USE_UDP", "使用 UDP 网络"),
                new MacroDefineInfo("USE_WEBSOCKET", "使用 WebSocket 网络"),
                new MacroDefineInfo("ENABLE_ENCRYPTION", "启用网络加密"),
            }
        },
        {
            "第三方库", new List<MacroDefineInfo>
            {
                new MacroDefineInfo("GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE", "Protobuf ref struct 兼容模式"),
                new MacroDefineInfo("DOTWEEN", "DOTween 动画库"),
                new MacroDefineInfo("UNITASK_DOTWEEN_SUPPORT", "UniTask 支持 DOTween"),
            }
        },
        {
            "发布相关", new List<MacroDefineInfo>
            {
                new MacroDefineInfo("RELEASE", "发布模式"),
                new MacroDefineInfo("DEVELOPMENT_BUILD", "开发版本构建"),
                new MacroDefineInfo("DISABLE_CHEATS", "禁用作弊功能"),
            }
        },
    };

    /// <summary>
    /// 宏定义信息
    /// </summary>
    private struct MacroDefineInfo
    {
        public string Name;
        public string Description;

        public MacroDefineInfo(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    /// <summary>
    /// 绘制宏定义配置
    /// </summary>
    private void DrawMacroDefinesSettings()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("宏定义设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("当前宏定义:");

        if (_currentMacroDefines.Count > 0)
        {
            List<string> toRemove = new List<string>();

            foreach (string macro in _currentMacroDefines)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(macro);
                if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(20)))
                {
                    toRemove.Add(macro);
                }
                EditorGUILayout.EndHorizontal();
            }

            // 删除标记的宏定义
            foreach (string macro in toRemove)
            {
                RemoveMacroDefine(macro);
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("无");
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(5);

        // 手动输入添加
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("添加宏定义:", GUILayout.Width(100));
        _macroDefinesInput = EditorGUILayout.TextField(_macroDefinesInput);
        if (GUILayout.Button("添加", GUILayout.Width(60)))
        {
            AddMacroDefine(_macroDefinesInput);
            _macroDefinesInput = "";
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // 预设宏定义选择
        DrawPresetMacrosSection();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制预设宏定义选择区域
    /// </summary>
    private void DrawPresetMacrosSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        _showPresetMacros = EditorGUILayout.Foldout(_showPresetMacros, "预设宏定义列表", true);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (_showPresetMacros)
        {
            GUILayout.FlexibleSpace();
            _presetMacrosScrollPos = EditorGUILayout.BeginScrollView(_presetMacrosScrollPos, GUILayout.MinHeight(350), GUILayout.MaxHeight(500));

            foreach (var category in PresetMacroCategories)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField($"【{category.Key}】", EditorStyles.boldLabel);

                // 每行显示多个宏定义按钮
                int itemsPerRow = 3;
                int count = 0;
                EditorGUILayout.BeginHorizontal();

                foreach (var macroInfo in category.Value)
                {
                    if (count > 0 && count % itemsPerRow == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    DrawPresetMacroButton(macroInfo);
                    count++;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制单个预设宏定义按钮
    /// </summary>
    private void DrawPresetMacroButton(MacroDefineInfo macroInfo)
    {
        bool isAdded = _currentMacroDefines.Contains(macroInfo.Name);

        var originalColor = GUI.backgroundColor;
        if (isAdded)
        {
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f); // 绿色表示已添加
        }

        // 创建带 tooltip 的按钮内容
        GUIContent buttonContent = new GUIContent(macroInfo.Name, macroInfo.Description);

        if (GUILayout.Button(buttonContent, GUILayout.MinWidth(180), GUILayout.Height(25)))
        {
            if (isAdded)
            {
                RemoveMacroDefine(macroInfo.Name);
            }
            else
            {
                AddMacroDefine(macroInfo.Name);
            }
        }

        GUI.backgroundColor = originalColor;
    }

    /// <summary>
    /// 加载当前宏定义
    /// </summary>
    private void LoadCurrentMacroDefines()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        _currentMacroDefines.Clear();
        if (!string.IsNullOrEmpty(defines))
        {
            _currentMacroDefines.AddRange(defines.Split(';'));
        }
    }

    /// <summary>
    /// 添加宏定义
    /// </summary>
    private void AddMacroDefine(string macroDefine)
    {
        if (string.IsNullOrEmpty(macroDefine)) return;

        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        string[] newDefines = macroDefine.Split(';');
        List<string> allDefines = new List<string>();

        if (!string.IsNullOrEmpty(currentDefines))
        {
            allDefines.AddRange(currentDefines.Split(';'));
        }

        foreach (string define in newDefines)
        {
            string trimmed = define.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !allDefines.Contains(trimmed))
            {
                allDefines.Add(trimmed);
            }
        }

        string newDefinesString = string.Join(";", allDefines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);

        LoadCurrentMacroDefines();
        Debug.Log($"[ConfigPanel] 宏定义已添加: {macroDefine}");
    }

    /// <summary>
    /// 清除所有宏定义
    /// </summary>
    private void ClearAllMacroDefines()
    {
        if (EditorUtility.DisplayDialog("确认", "确定要清除所有宏定义吗？", "确定", "取消"))
        {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "");
            LoadCurrentMacroDefines();
            Debug.Log($"[ConfigPanel] 所有宏定义已清除");
        }
    }

    /// <summary>
    /// 移除单个宏定义
    /// </summary>
    private void RemoveMacroDefine(string macroDefine)
    {
        if (string.IsNullOrEmpty(macroDefine)) return;

        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        List<string> allDefines = new List<string>();
        if (!string.IsNullOrEmpty(currentDefines))
        {
            allDefines.AddRange(currentDefines.Split(';'));
        }

        allDefines.Remove(macroDefine);

        string newDefinesString = string.Join(";", allDefines);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);

        LoadCurrentMacroDefines();
        Debug.Log($"[ConfigPanel] 宏定义已移除: {macroDefine}");
    }
}

}
