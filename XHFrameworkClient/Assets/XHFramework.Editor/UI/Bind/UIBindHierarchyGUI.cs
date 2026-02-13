using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using XHFramework.Core;

namespace XHFramework.Editor
{
    /// <summary>
    /// Hierarchy 面板绑定工具
    /// 挂载 BindData 组件的对象，其子物体会显示绑定按钮
    /// </summary>
    [InitializeOnLoad]
    public static class UIBindHierarchyGUI
    {
        // 不显示的组件类型
        private static readonly HashSet<string> IgnoredComponents = new HashSet<string>
        {
            "CanvasRenderer",
        };

        // BindData 颜色列表（用于区分不同层级）
        private static readonly Color[] BindDataColors = new Color[]
        {
            new Color(1f, 0.6f, 0.2f),      // 橙色
            new Color(0.4f, 0.8f, 1f),      // 青色
            new Color(0.8f, 0.5f, 1f),      // 紫色
            new Color(0.5f, 1f, 0.5f),      // 绿色
            new Color(1f, 0.5f, 0.7f),      // 粉色
            new Color(1f, 1f, 0.4f),        // 黄色
            new Color(0.6f, 0.8f, 0.6f),    // 浅绿
            new Color(0.8f, 0.6f, 0.4f),    // 棕色
        };

        // BindData InstanceID 到颜色索引的映射
        private static Dictionary<int, int> _bindDataColorIndex = new Dictionary<int, int>();
        private static int _nextColorIndex = 0;

        static UIBindHierarchyGUI()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            ObjectFactory.componentWasAdded += OnComponentAdded;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// 当 Hierarchy 改变时重置颜色映射
        /// </summary>
        private static void OnHierarchyChanged()
        {
            // 清理不存在的 BindData
            var toRemove = new List<int>();
            foreach (var kvp in _bindDataColorIndex)
            {
                if (EditorUtility.InstanceIDToObject(kvp.Key) == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var id in toRemove)
            {
                _bindDataColorIndex.Remove(id);
            }
        }

        /// <summary>
        /// 获取 BindData 对应的颜色
        /// </summary>
        private static Color GetBindDataColor(BindData bindData)
        {
            int instanceID = bindData.GetInstanceID();
            if (!_bindDataColorIndex.TryGetValue(instanceID, out int colorIndex))
            {
                colorIndex = _nextColorIndex % BindDataColors.Length;
                _bindDataColorIndex[instanceID] = colorIndex;
                _nextColorIndex++;
            }
            return BindDataColors[colorIndex];
        }

        /// <summary>
        /// 当组件被添加时（处理中间层级添加 BindData 的情况）
        /// </summary>
        private static void OnComponentAdded(Component component)
        {
            if (component is BindData newBindData)
            {
                // 查找父级的 BindData
                BindData parentBindData = FindOwnerBindData(component.transform);
                if (parentBindData == null) return;

                // 把属于新 BindData 子物体的绑定从父级移过来
                var parentBindings = parentBindData.GetBindings();
                var toMove = new List<Component>();

                foreach (var binding in parentBindings)
                {
                    if (binding != null && IsChildOf(binding.transform, newBindData.transform))
                    {
                        toMove.Add(binding);
                    }
                }

                if (toMove.Count > 0)
                {
                    Undo.RecordObject(parentBindData, "Move Bindings");
                    Undo.RecordObject(newBindData, "Move Bindings");

                    foreach (var binding in toMove)
                    {
                        parentBindData.RemoveBinding(binding);
                        newBindData.AddBinding(binding);
                    }

                    EditorUtility.SetDirty(parentBindData);
                    EditorUtility.SetDirty(newBindData);

                    Debug.Log($"[Bind] 已将 {toMove.Count} 个绑定从 {parentBindData.name} 移动到 {newBindData.name}");
                }
            }
        }

        /// <summary>
        /// 检查 target 是否是 parent 的子物体
        /// </summary>
        private static bool IsChildOf(Transform target, Transform parent)
        {
            Transform current = target;
            while (current != null)
            {
                if (current == parent) return true;
                current = current.parent;
            }
            return false;
        }

        /// <summary>
        /// Hierarchy 面板绘制回调
        /// </summary>
        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            float buttonWidth = 20f;
            float spacing = 2f;
            float rightMargin = 5f;
            float xPos = selectionRect.xMax - rightMargin - buttonWidth;

            // 检查自身是否有 BindData
            var selfBindData = go.GetComponent<BindData>();

            // 查找该物体所属的上层 BindData
            BindData ownerBindData = FindOwnerBindData(go.transform);

            // 保存原始颜色
            Color originalColor = GUI.backgroundColor;

            // 先绘制 ★ 标志（最右边）
            if (selfBindData != null)
            {
                DrawBindDataStar(selectionRect, selfBindData, ref xPos, spacing);
            }

            // 然后绘制 [+] 按钮（在 ★ 左边，保持对齐）
            if (ownerBindData != null)
            {
                Color bindColor = GetBindDataColor(ownerBindData);
                GUI.backgroundColor = bindColor;
                Rect addButtonRect = new Rect(xPos, selectionRect.y, buttonWidth, selectionRect.height);
                if (GUI.Button(addButtonRect, "+", EditorStyles.miniButton))
                {
                    ShowAddComponentMenu(go, ownerBindData);
                }
                xPos -= (buttonWidth + spacing);
                GUI.backgroundColor = originalColor;
            }

            // 最后绘制已绑定的组件按钮
            if (ownerBindData != null)
            {
                Color bindColor = GetBindDataColor(ownerBindData);
                var boundTypes = GetBoundComponentTypes(go, ownerBindData);

                GUI.backgroundColor = bindColor;
                foreach (var typeName in boundTypes)
                {
                    string displayName = GetButtonDisplayName(typeName);
                    float btnWidth = GUI.skin.button.CalcSize(new GUIContent(displayName)).x + 4;
                    btnWidth = Mathf.Max(buttonWidth, btnWidth);
                    Rect bindingButtonRect = new Rect(xPos - btnWidth + buttonWidth, selectionRect.y, btnWidth, selectionRect.height);

                    if (GUI.Button(bindingButtonRect, displayName, EditorStyles.miniButton))
                    {
                        ShowBindingOptionsMenu(go, ownerBindData, typeName);
                    }
                    xPos -= (btnWidth + spacing);
                }
                GUI.backgroundColor = originalColor;
            }
        }

        /// <summary>
        /// 绘制 BindData 对象的 ★ 标志
        /// </summary>
        private static void DrawBindDataStar(Rect selectionRect, BindData bindData, ref float xPos, float spacing)
        {
            Color starColor = GetBindDataColor(bindData);
            float starWidth = 20f;

            Rect starRect = new Rect(xPos, selectionRect.y, starWidth, selectionRect.height);

            // 保存原始颜色
            Color originalColor = GUI.contentColor;
            GUI.contentColor = starColor;

            // 绘制五角星
            GUIStyle starStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(starRect, "★", starStyle);

            GUI.contentColor = originalColor;

            xPos -= (starWidth + spacing);
        }

        /// <summary>
        /// 查找物体所属的 BindData（最近的父级）
        /// </summary>
        private static BindData FindOwnerBindData(Transform transform)
        {
            Transform current = transform.parent;

            while (current != null)
            {
                var bindData = current.GetComponent<BindData>();
                if (bindData != null)
                {
                    return bindData;
                }
                current = current.parent;
            }

            return null;
        }

        /// <summary>
        /// 获取某个 GameObject 已绑定的组件类型列表
        /// </summary>
        private static List<string> GetBoundComponentTypes(GameObject go, BindData bindData)
        {
            var result = new List<string>();
            var bindings = bindData.GetBindings();

            foreach (var binding in bindings)
            {
                if (binding != null && binding.gameObject == go)
                {
                    string typeName = binding.GetType().Name;
                    result.Add(typeName);
                }
            }

            return result;
        }

        /// <summary>
        /// 显示添加组件菜单
        /// </summary>
        private static void ShowAddComponentMenu(GameObject go, BindData bindData)
        {
            GenericMenu menu = new GenericMenu();

            var boundTypes = GetBoundComponentTypes(go, bindData);

            // 添加 GameObject 选项（用 Transform 表示）
            string transformType = go.GetComponent<RectTransform>() != null ? "RectTransform" : "Transform";
            if (boundTypes.Contains(transformType))
            {
                menu.AddDisabledItem(new GUIContent($"GameObject ({transformType}) (已绑定)"));
            }
            else
            {
                menu.AddItem(new GUIContent($"GameObject ({transformType})"), false, () => AddBinding(go, bindData, transformType));
            }
            menu.AddSeparator("");

            // 获取所有组件
            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;

                string typeName = component.GetType().Name;
                if (IgnoredComponents.Contains(typeName)) continue;
                if (typeName == "Transform" || typeName == "RectTransform") continue; // 已在上面处理

                if (boundTypes.Contains(typeName))
                {
                    menu.AddDisabledItem(new GUIContent($"{typeName} (已绑定)"));
                }
                else
                {
                    string componentTypeName = typeName;
                    menu.AddItem(new GUIContent(typeName), false, () => AddBinding(go, bindData, componentTypeName));
                }
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// 显示绑定选项菜单
        /// </summary>
        private static void ShowBindingOptionsMenu(GameObject go, BindData bindData, string componentType)
        {
            GenericMenu menu = new GenericMenu();

            // 移除选项
            menu.AddItem(new GUIContent("移除绑定"), false, () => RemoveBinding(go, bindData, componentType));
            menu.AddSeparator("");

            var boundTypes = GetBoundComponentTypes(go, bindData);

            // 替换为其他组件
            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;

                string typeName = component.GetType().Name;
                if (IgnoredComponents.Contains(typeName)) continue;
                if (typeName == componentType) continue;

                if (!boundTypes.Contains(typeName))
                {
                    string newTypeName = typeName;
                    menu.AddItem(new GUIContent($"替换为 {typeName}"), false,
                        () => ReplaceBinding(go, bindData, componentType, newTypeName));
                }
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// 添加绑定
        /// </summary>
        private static void AddBinding(GameObject go, BindData bindData, string componentType)
        {
            Component target = go.GetComponent(componentType);

            if (target == null)
            {
                Debug.LogWarning($"[Bind] 找不到组件: {componentType}");
                return;
            }

            Undo.RecordObject(bindData, "Add Binding");
            bindData.AddBinding(target);
            EditorUtility.SetDirty(bindData);

            Debug.Log($"[Bind] 添加绑定: {go.name}.{componentType}");
        }

        /// <summary>
        /// 移除绑定
        /// </summary>
        private static void RemoveBinding(GameObject go, BindData bindData, string componentType)
        {
            Component target = go.GetComponent(componentType);
            if (target == null) return;

            Undo.RecordObject(bindData, "Remove Binding");
            bindData.RemoveBinding(target);
            EditorUtility.SetDirty(bindData);

            Debug.Log($"[Bind] 移除绑定: {go.name}.{componentType}");
        }

        /// <summary>
        /// 替换绑定
        /// </summary>
        private static void ReplaceBinding(GameObject go, BindData bindData, string oldComponentType, string newComponentType)
        {
            Component oldTarget = go.GetComponent(oldComponentType);
            Component newTarget = go.GetComponent(newComponentType);

            if (oldTarget == null || newTarget == null) return;

            Undo.RecordObject(bindData, "Replace Binding");
            bindData.RemoveBinding(oldTarget);
            bindData.AddBinding(newTarget);
            EditorUtility.SetDirty(bindData);

            Debug.Log($"[Bind] 替换绑定: {go.name}.{oldComponentType} -> {newComponentType}");
        }

        /// <summary>
        /// 生成字段名（供代码生成器使用）
        /// 格式: _类型_名称
        /// </summary>
        public static string GenerateFieldName(string goName, string componentType)
        {
            // Transform/RectTransform 作为 GameObject 绑定时使用 GameObject 类型名
            string typeName = (componentType == "Transform" || componentType == "RectTransform")
                ? "GameObject"
                : componentType;

            return $"_{typeName}_{goName}";
        }

        /// <summary>
        /// 获取按钮显示名称（完整类型名）
        /// </summary>
        public static string GetButtonDisplayName(string componentType)
        {
            // Transform/RectTransform 显示为 GameObject
            if (componentType == "Transform" || componentType == "RectTransform")
            {
                return "GameObject";
            }
            return componentType;
        }
    }
}
