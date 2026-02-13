using System.Collections.Generic;
using System.Linq;
using  XHFramework.Game;
using Luban;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using XHFramework.Core;

namespace XHFramework.Editor {

/// <summary>
/// LocalizationTextBind编辑器
/// 提供本地化文本选择UI
/// </summary>
[CustomEditor(typeof(LocalizationTextBind))]
public class LocalizationTextBindEditor : UnityEditor.Editor
{
    private SerializedProperty _selectedId;
    private SerializedProperty _selectedCN;
    private SerializedProperty _tmpComponent;
    private SerializedProperty _textComponent;

    private void OnEnable()
    {
        _selectedId = serializedObject.FindProperty("_selectedId");
        _selectedCN = serializedObject.FindProperty("_selectedCN");
        _tmpComponent = serializedObject.FindProperty("_tmpComponent");
        _textComponent = serializedObject.FindProperty("_textComponent");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        LocalizationTextBind bind = (LocalizationTextBind)target;

        EditorGUILayout.Space(10);

        // 显示当前绑定信息
        EditorGUILayout.LabelField("本地化绑定信息", EditorStyles.boldLabel);

        // 根据组件类型显示对应的字段
        if (bind.TmpComponent != null)
        {
            EditorGUILayout.PropertyField(_tmpComponent, new GUIContent("TMP文本组件"));
        }
        else if (bind.TextComponent != null)
        {
            EditorGUILayout.PropertyField(_textComponent, new GUIContent("Text文本组件"));
        }
        else
        {
            EditorGUILayout.HelpBox("未找到文本组件 (TextMeshProUGUI 或 Text)", MessageType.Warning);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("选中的ID", GUILayout.Width(100));
        EditorGUILayout.LabelField(_selectedId.intValue.ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("中文", GUILayout.Width(100));
        EditorGUILayout.LabelField(_selectedCN.stringValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 选择本地化文本按钮和重置按钮
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("选择本地化文本", GUILayout.Height(30)))
        {
            LocalizationTextSelectionWindow.ShowWindow(bind);
        }
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = new Color(0.8f, 0.6f, 0.4f);
        if (GUILayout.Button("重置", GUILayout.Height(30), GUILayout.Width(80)))
        {
            ResetLocalizationText(bind);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 重置本地化文本（根据当前ID重新读表设置）
    /// </summary>
    private void ResetLocalizationText(LocalizationTextBind bind)
    {
        if (bind.SelectedId <= 0)
        {
            Debug.LogWarning("[LocalizationTextBind] 未选择有效的本地化文本ID");
            return;
        }

        try
        {
            TbLocalizationText data = GetLocalConfigData("tblocalizationtext");
            if (data.DataList != null)
            {
                var item = data.DataList.Find(x => x.Id == bind.SelectedId);
                if (item != null)
                {
                    Undo.RecordObject(bind, "Reset Localization Text");

                    if (bind.TmpComponent != null)
                        Undo.RecordObject(bind.TmpComponent, "Update Text");
                    if (bind.TextComponent != null)
                        Undo.RecordObject(bind.TextComponent, "Update Text");

                    bind.SetLocalizationText(item.Id, item.CN, item.CN);

                    EditorUtility.SetDirty(bind);
                    if (bind.TmpComponent != null)
                        EditorUtility.SetDirty(bind.TmpComponent);
                    if (bind.TextComponent != null)
                        EditorUtility.SetDirty(bind.TextComponent);

                    Debug.Log($"[LocalizationTextBind] 重置本地化文本成功: ID={item.Id}, CN={item.CN}");
                }
                else
                {
                    Debug.LogError($"[LocalizationTextBind] 未找到ID为 {bind.SelectedId} 的本地化文本");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LocalizationTextBind] 重置失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载本地化配置数据
    /// </summary>
    public static TbLocalizationText GetLocalConfigData(string fileName)
    {
        var fullPath =ResourceConfig.GetDataTableAsset(fileName) ;
        TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
        TbLocalizationText tbLocalizationText = new TbLocalizationText();
        tbLocalizationText.LoadData(new ByteBuf(text.bytes));
        return tbLocalizationText;
    }
}

/// <summary>
/// 本地化文本选择窗口
/// </summary>
public class LocalizationTextSelectionWindow : EditorWindow
{
    private LocalizationTextBind _targetBind;
    private List<LocalizationTextItem> _allItems;
    private List<LocalizationTextItem> _filteredItems;
    private string _searchText = "";
    private Vector2 _scrollPosition;

    private class LocalizationTextItem
    {
        public int Id;
        public string Key;
        public string CN;
        public string EN;
    }

    public static void ShowWindow(LocalizationTextBind bind)
    {
        LocalizationTextSelectionWindow window = GetWindow<LocalizationTextSelectionWindow>("选择本地化文本");
        window.minSize = new Vector2(800, 600);
        window._targetBind = bind;
        window.LoadData();
        window.Show();
    }

    private void LoadData()
    {
        try
        {
            TbLocalizationText data = LocalizationTextBindEditor.GetLocalConfigData("tblocalizationtext");
            _allItems = new List<LocalizationTextItem>();
        
            if (data.DataList != null)
            {
                foreach (var item in data.DataList)
                {
                    _allItems.Add(new LocalizationTextItem
                    {
                        Id = item.Id,
                        Key = item.Key,
                        CN = item.CN,
                        EN = item.EN
                    });
                }
            }

            _filteredItems = new List<LocalizationTextItem>(_allItems);
            Debug.Log($"[LocalizationTextBind] 加载了 {_allItems.Count} 条本地化文本数据");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LocalizationTextBind] 加载数据失败: {e.Message}");
            _allItems = new List<LocalizationTextItem>();
            _filteredItems = new List<LocalizationTextItem>();
        }
    }

    private void OnGUI()
    {
        if (_targetBind == null)
        {
            EditorGUILayout.HelpBox("目标组件已丢失", MessageType.Error);
            return;
        }

        EditorGUILayout.Space(10);

        // 搜索框
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索Key:", GUILayout.Width(70));
        string newSearchText = EditorGUILayout.TextField(_searchText);
        if (newSearchText != _searchText)
        {
            _searchText = newSearchText;
            FilterItems();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // 显示结果数量
        EditorGUILayout.LabelField($"共 {_filteredItems.Count} 条结果");

        EditorGUILayout.Space(5);

        // 表头
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(60));
        EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("中文", EditorStyles.boldLabel, GUILayout.Width(250));
        EditorGUILayout.LabelField("英文", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        // 滚动列表
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = 0; i < _filteredItems.Count; i++)
        {
            var item = _filteredItems[i];

            EditorGUILayout.BeginHorizontal();

            // 显示数据
            EditorGUILayout.LabelField(item.Id.ToString(), GUILayout.Width(60));
            EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
            EditorGUILayout.LabelField(item.CN, GUILayout.Width(250));
            EditorGUILayout.LabelField(item.EN);

            // 选择按钮
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                SelectItem(item);
            }

            EditorGUILayout.EndHorizontal();

            // 绘制分隔线
            if (i < _filteredItems.Count - 1)
            {
                EditorGUILayout.Space(2);
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void FilterItems()
    {
        if (string.IsNullOrEmpty(_searchText))
        {
            _filteredItems = new List<LocalizationTextItem>(_allItems);
        }
        else
        {
            _filteredItems = _allItems
                .Where(item => item.Key.Contains(_searchText))
                .ToList();
        }
    }

    private void SelectItem(LocalizationTextItem item)
    {
        if (_targetBind != null)
        {
            bool hasComponent = _targetBind.TmpComponent != null || _targetBind.TextComponent != null;

            if (!hasComponent)
            {
                Debug.LogError("[LocalizationTextBind] 没有找到有效的文本组件");
                return;
            }

            Undo.RecordObject(_targetBind, "Select Localization Text");

            if (_targetBind.TmpComponent != null)
                Undo.RecordObject(_targetBind.TmpComponent, "Update Text");
            if (_targetBind.TextComponent != null)
                Undo.RecordObject(_targetBind.TextComponent, "Update Text");

            _targetBind.SetLocalizationText(item.Id, item.CN, item.CN);

            EditorUtility.SetDirty(_targetBind);
            if (_targetBind.TmpComponent != null)
                EditorUtility.SetDirty(_targetBind.TmpComponent);
            if (_targetBind.TextComponent != null)
                EditorUtility.SetDirty(_targetBind.TextComponent);

            Debug.Log($"[LocalizationTextBind] 选择了本地化文本: ID={item.Id}, Key={item.Key}");

            Close();
        }
    }
}

}
