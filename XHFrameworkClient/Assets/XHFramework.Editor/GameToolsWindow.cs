using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace XHFramework.Editor {

/// <summary>
/// 游戏工具主窗口 - 重构为更加面向对象的架构
/// </summary>
public class GameToolsWindow : EditorWindow
{
    // 工具面板注册表
    private List<BaseToolPanel> _toolPanels;
    private int _selectedPanelIndex = 0;
    private Vector2 _leftScrollPosition = Vector2.zero;
    private Vector2 _rightScrollPosition = Vector2.zero;

    // UI样式
    private GUIStyle _selectedButtonStyle;
    private GUIStyle _normalButtonStyle;

    [MenuItem("XHFramework/游戏工具")]
    public static void OpenWindow()
    {
        var window = GetWindow<GameToolsWindow>("🎮 游戏工具");
        window.minSize = new Vector2(900, 650);
        window.Show();
    }

    void OnEnable()
    {
        InitializeToolPanels();
       
    }

    void OnDisable()
    {
        // 通知所有面板失活
        _toolPanels?.ForEach(panel => panel.OnDisable());
    }

    void OnDestroy()
    {
        // 清理所有面板
        _toolPanels?.ForEach(panel => panel.OnDestroy());
        // Log 系统有自动 Shutdown 机制，无需手动调用
    }

    void OnGUI()
    {
        if (_toolPanels == null)
        {
            InitializeToolPanels();
        }

        if (_selectedButtonStyle == null || _normalButtonStyle == null)
        {
            InitializeStyles();
        }

        try
        {
            EditorGUILayout.BeginHorizontal();

            // 左侧工具列表面板
            DrawLeftPanel();

            // 分割线
            DrawSeparator();

            // 右侧工具内容面板
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();
        }
        catch (System.Exception e)
        {
            EditorGUILayout.EndHorizontal();
            Debug.LogError($"GameToolsWindow OnGUI Error: {e.Message}");
            EditorGUILayout.HelpBox($"窗口绘制错误: {e.Message}", MessageType.Error);
        }
    }

    /// <summary>
    /// 初始化工具面板
    /// </summary>
    private void InitializeToolPanels()
    {
        try
        {
            _toolPanels = new List<BaseToolPanel>
            {
                
                new DataTableToolPanel(),
                new NetworkToolPanel(),
                new ConfigPanel(),
                new BuildToolPanel(),
                
                // 未来可以在这里添加更多面板
                // new DebugToolPanel(),
                // new DeployToolPanel()
            };

            // 激活当前选中的面板
            if (_toolPanels.Count > 0 && _selectedPanelIndex >= 0 && _selectedPanelIndex < _toolPanels.Count)
            {
                _toolPanels[_selectedPanelIndex].OnEnable();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"初始化工具面板失败: {e.Message}");
            _toolPanels = new List<BaseToolPanel>(); // 创建空列表避免空引用
        }
    }

    /// <summary>
    /// 初始化UI样式
    /// </summary>
    private void InitializeStyles()
    {
        try
        {
            // 只有在OnGUI上下文中才能访问GUI.skin
            if (Event.current != null)
            {
                _selectedButtonStyle = new GUIStyle(GUI.skin.button);
                var backgroundTexture = MakeTexture(new Color(0.3f, 0.5f, 1f, 0.8f));
                if (backgroundTexture != null)
                {
                    _selectedButtonStyle.normal.background = backgroundTexture;
                }
                _selectedButtonStyle.fontStyle = FontStyle.Bold;

                _normalButtonStyle = new GUIStyle(GUI.skin.button);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"初始化样式时出错: {e.Message}");
            // 使用null，在绘制时会使用默认样式
            _selectedButtonStyle = null;
            _normalButtonStyle = null;
        }
    }

    /// <summary>
    /// 绘制左侧工具面板
    /// </summary>
    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220), GUILayout.ExpandHeight(true));

        // 标题区域
        DrawLeftPanelHeader();

        // 工具列表区域
        DrawToolList();

        // 底部信息区域
        DrawLeftPanelFooter();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制左侧面板标题
    /// </summary>
    private void DrawLeftPanelHeader()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("🛠️ 工具面板", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
    }

    /// <summary>
    /// 绘制工具列表
    /// </summary>
    private void DrawToolList()
    {
        GUILayout.Label("📋 可用工具", EditorStyles.boldLabel);
        GUILayout.Space(5);

        _leftScrollPosition = EditorGUILayout.BeginScrollView(_leftScrollPosition);

        if (_toolPanels != null && _toolPanels.Count > 0)
        {
            for (int i = 0; i < _toolPanels.Count; i++)
            {
                try
                {
                    var panel = _toolPanels[i];
                    if (panel == null) continue;

                    var isSelected = _selectedPanelIndex == i;
                    var style = isSelected ? (_selectedButtonStyle ?? GUI.skin.button) : (_normalButtonStyle ?? GUI.skin.button);

                    EditorGUILayout.BeginHorizontal();

                    // 工具按钮
                    if (GUILayout.Button($"{panel.PanelIcon} {panel.PanelName}", style, GUILayout.Height(40)))
                    {
                        SelectPanel(i);
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"绘制工具面板 {i} 时出错: {e.Message}");
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("没有可用的工具面板", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制左侧面板底部
    /// </summary>
    private void DrawLeftPanelFooter()
    {
        GUILayout.FlexibleSpace();
        // 移除底部提示信息
    }

    /// <summary>
    /// 绘制分割线
    /// </summary>
    private void DrawSeparator()
    {
        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(2), GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        GUILayout.Space(8);
    }

    /// <summary>
    /// 绘制右侧内容面板
    /// </summary>
    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (_toolPanels != null && _selectedPanelIndex >= 0 && _selectedPanelIndex < _toolPanels.Count)
        {
            var selectedPanel = _toolPanels[_selectedPanelIndex];

            if (selectedPanel != null)
            {
                _rightScrollPosition = EditorGUILayout.BeginScrollView(_rightScrollPosition);

                try
                {
                    // 绘制选中的工具面板
                    selectedPanel.OnGUI();
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.HelpBox($"面板绘制错误: {e.Message}", MessageType.Error);
                    Debug.LogError($"Tool panel error: {e}");
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("选中的工具面板为空", MessageType.Warning);
            }
        }
        else
        {
            DrawEmptyPanel();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制空面板
    /// </summary>
    private void DrawEmptyPanel()
    {
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("🎮", new GUIStyle(EditorStyles.largeLabel) { fontSize = 48, alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(10);
        GUILayout.Label("欢迎使用游戏工具", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Label("请从左侧选择一个工具开始使用", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
    }

    /// <summary>
    /// 选择工具面板
    /// </summary>
    private void SelectPanel(int index)
    {
        if (index == _selectedPanelIndex || _toolPanels == null) return;

        try
        {
            // 失活当前面板
            if (_selectedPanelIndex >= 0 && _selectedPanelIndex < _toolPanels.Count)
            {
                var currentPanel = _toolPanels[_selectedPanelIndex];
                if (currentPanel != null)
                {
                    currentPanel.OnDisable();
                }
            }

            // 激活新面板
            _selectedPanelIndex = index;
            if (_selectedPanelIndex >= 0 && _selectedPanelIndex < _toolPanels.Count)
            {
                var newPanel = _toolPanels[_selectedPanelIndex];
                if (newPanel != null)
                {
                    newPanel.OnEnable();
                }
            }

            // 重置右侧滚动位置
            _rightScrollPosition = Vector2.zero;

            // 重绘窗口
            Repaint();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"选择面板时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 创建纯色材质
    /// </summary>
    private Texture2D MakeTexture(Color color)
    {
        try
        {
            Color[] pixels = new Color[1];
            pixels[0] = color;

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"创建材质失败: {e.Message}");
            return null;
        }
    }
}

}