using UnityEngine;
using UnityEditor;
using XHFramework.Core;

namespace XHFramework.Editor
{
    /// <summary>
    /// BindData 自定义 Inspector
    /// </summary>
    [CustomEditor(typeof(BindData))]
    public class BindDataEditor : UnityEditor.Editor
    {
        private BindData _bindData;
        private SerializedProperty _targetScriptProp;

        private void OnEnable()
        {
            _bindData = (BindData)target;
            _targetScriptProp = serializedObject.FindProperty("_targetScript");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 目标脚本选择
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("目标脚本", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_targetScriptProp, new GUIContent("Script"));

            if (_targetScriptProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("请选择要生成 partial 类的目标脚本", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // 按钮行
            EditorGUILayout.BeginHorizontal();

            // 生成代码按钮
            EditorGUI.BeginDisabledGroup(_targetScriptProp.objectReferenceValue == null);
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("生成 Bind 脚本", GUILayout.Height(30)))
            {
                // 先应用修改，确保序列化数据是最新的
                serializedObject.ApplyModifiedProperties();

                // 强制刷新 BindData 引用
                _bindData = (BindData)target;

                // 调试输出，确认使用的是哪个脚本
                if (_bindData.TargetScript != null)
                {
                    Debug.Log($"[Bind] 准备生成代码，目标脚本: {_bindData.TargetScript.name}, 类名: {_bindData.TargetScript.GetClass()?.Name}");
                }

                UIBindCodeGenerator.GenerateCode(_bindData);
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();

            // 刷新按钮
            GUI.backgroundColor = new Color(0.6f, 0.8f, 1f);
            if (GUILayout.Button("刷新", GUILayout.Height(30), GUILayout.Width(60)))
            {
                RefreshBindings();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 绑定数量统计
            var bindings = _bindData.GetBindings();
            EditorGUILayout.HelpBox($"绑定数量: {bindings.Count}", MessageType.Info);

            EditorGUILayout.Space(10);

            // 绘制绑定列表
            EditorGUILayout.LabelField("绑定列表", EditorStyles.boldLabel);

            if (bindings.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无绑定，请在 Hierarchy 面板中点击子物体的 [+] 按钮添加绑定", MessageType.None);
            }
            else
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    EditorGUILayout.BeginHorizontal();

                    // 索引
                    EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));

                    // 组件引用
                    EditorGUI.BeginDisabledGroup(true);
                    if (binding != null)
                    {
                        string typeName = binding.GetType().Name;
                        string fieldName = UIBindHierarchyGUI.GenerateFieldName(binding.gameObject.name, typeName);
                        EditorGUILayout.LabelField(fieldName, GUILayout.Width(150));
                        EditorGUILayout.ObjectField(binding, typeof(Component), true);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("(Missing)", GUILayout.Width(150));
                        EditorGUILayout.LabelField("null");
                    }
                    EditorGUI.EndDisabledGroup();

                    // 删除按钮
                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        Undo.RecordObject(_bindData, "Remove Binding");
                        _bindData.RemoveBindingAt(i);
                        EditorUtility.SetDirty(_bindData);
                        break;
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // 清空按钮
            if (bindings.Count > 0)
            {
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("清空所有绑定"))
                {
                    if (EditorUtility.DisplayDialog("确认", "确定要清空所有绑定吗？", "确定", "取消"))
                    {
                        Undo.RecordObject(_bindData, "Clear Bindings");
                        _bindData.ClearBindings();
                        EditorUtility.SetDirty(_bindData);
                    }
                }
                GUI.backgroundColor = Color.white;
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 刷新绑定，清理无效的绑定
        /// </summary>
        private void RefreshBindings()
        {
            var bindings = _bindData.GetBindings();
            int removedCount = 0;

            Undo.RecordObject(_bindData, "Refresh Bindings");

            // 从后往前遍历，移除无效绑定
            for (int i = bindings.Count - 1; i >= 0; i--)
            {
                if (bindings[i] == null)
                {
                    _bindData.RemoveBindingAt(i);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                EditorUtility.SetDirty(_bindData);
                Debug.Log($"[Bind] 已清理 {removedCount} 个无效绑定");
            }
            else
            {
                Debug.Log("[Bind] 没有无效绑定需要清理");
            }
        }
    }
}
