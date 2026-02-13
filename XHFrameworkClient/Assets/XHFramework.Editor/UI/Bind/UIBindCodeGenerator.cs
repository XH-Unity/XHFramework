using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using XHFramework.Core;

namespace XHFramework.Editor
{
    /// <summary>
    /// 绑定代码生成器
    /// </summary>
    public static class UIBindCodeGenerator
    {

        /// <summary>
        /// 从 BindData 生成绑定代码
        /// </summary>
        public static void GenerateCode(BindData bindData)
        {
            // 检查是否选择了目标脚本
            if (bindData.TargetScript == null)
            {
                Debug.LogError("[Bind] 请先选择目标脚本");
                return;
            }

            var bindings = bindData.GetBindings();
            if (bindings == null || bindings.Count == 0)
            {
                Debug.LogWarning($"[Bind] {bindData.gameObject.name} 没有绑定数据");
                return;
            }

            // 从选择的脚本获取信息
            var scriptClass = bindData.TargetScript.GetClass();
            if (scriptClass == null)
            {
                Debug.LogError($"[Bind] 无法获取脚本类型，脚本资源: {bindData.TargetScript.name}");
                return;
            }

            string className = scriptClass.Name;
            string mainScriptPath = AssetDatabase.GetAssetPath(bindData.TargetScript);

            Debug.Log($"[Bind] 开始生成代码 - GameObject: {bindData.gameObject.name}, 目标脚本: {bindData.TargetScript.name}, 类名: {className}, 路径: {mainScriptPath}");

            if (string.IsNullOrEmpty(mainScriptPath))
            {
                Debug.LogError($"[Bind] 无法获取脚本路径: {className}");
                return;
            }

            // 确保主脚本有 partial 修饰符
            EnsurePartialClass(mainScriptPath, className);

            // 读取主脚本获取命名空间
            string mainScriptContent = File.ReadAllText(mainScriptPath);
            string ns = ExtractNamespace(mainScriptContent);

            // 生成 .Bind.cs 文件路径
            string directory = Path.GetDirectoryName(mainScriptPath);
            string bindFilePath = Path.Combine(directory, $"{className}.Bind.cs");

            // 生成绑定代码
            string bindCode = GenerateBindFile(className, ns, bindings);
            File.WriteAllText(bindFilePath, bindCode, Encoding.UTF8);

            AssetDatabase.Refresh();

            Debug.Log($"[Bind] 代码已生成: {bindFilePath}");
        }

        /// <summary>
        /// 确保类有 partial 修饰符
        /// </summary>
        private static void EnsurePartialClass(string filePath, string className)
        {
            string content = File.ReadAllText(filePath);

            if (Regex.IsMatch(content, $@"\bpartial\s+class\s+{className}\b"))
            {
                return;
            }

            string pattern = $@"(\bpublic\s+)(class\s+{className}\b)";
            if (Regex.IsMatch(content, pattern))
            {
                content = Regex.Replace(content, pattern, "$1partial $2");
                File.WriteAllText(filePath, content, Encoding.UTF8);
                Debug.Log($"[Bind] 已添加 partial 修饰符: {filePath}");
            }
        }

        /// <summary>
        /// 提取命名空间
        /// </summary>
        private static string ExtractNamespace(string content)
        {
            var match = Regex.Match(content, @"namespace\s+([\w.]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        /// <summary>
        /// 生成绑定文件内容
        /// </summary>
        private static string GenerateBindFile(string className, string ns, List<Component> bindings)
        {
            var sb = new StringBuilder();

            // 收集需要的 using（通过反射自动获取命名空间）
            var usings = new HashSet<string> { "UnityEngine", "XHFramework.Core" };

            foreach (var binding in bindings)
            {
                if (binding == null) continue;

                // 通过反射获取组件的命名空间
                var componentType = binding.GetType();
                if (!string.IsNullOrEmpty(componentType.Namespace))
                {
                    usings.Add(componentType.Namespace);
                }
            }

            // 文件头
            sb.AppendLine("// ==========================================================");
            sb.AppendLine("// 此文件由工具自动生成，请勿手动修改");
            sb.AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("// ==========================================================");
            sb.AppendLine();

            // using 语句
            foreach (var u in usings.OrderBy(x => x))
            {
                sb.AppendLine($"using {u};");
            }
            sb.AppendLine();

            // 命名空间
            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                sb.AppendLine();
            }

            // 类定义
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");

            // 字段定义
            sb.AppendLine("    #region 自动绑定字段");
            sb.AppendLine();

            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null) continue;

                string typeName = binding.GetType().Name;
                string goName = binding.gameObject.name;
                string fieldName = UIBindHierarchyGUI.GenerateFieldName(goName, typeName);

                // Transform/RectTransform 生成 GameObject 字段
                if (typeName == "Transform" || typeName == "RectTransform")
                {
                    sb.AppendLine($"    protected GameObject {fieldName};");
                }
                else
                {
                    sb.AppendLine($"    protected {typeName} {fieldName};");
                }
            }

            sb.AppendLine();
            sb.AppendLine("    #endregion");
            sb.AppendLine();

            // BindInit 方法
            sb.AppendLine("    #region 自动绑定初始化");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 自动绑定初始化");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    protected override void BindInit()");
            sb.AppendLine("    {");

            if (bindings.Count > 0)
            {
                sb.AppendLine("        var bindData = gameObject.GetComponent<BindData>();");
                sb.AppendLine("        if (bindData == null)");
                sb.AppendLine("        {");
                sb.AppendLine($"            UnityEngine.Debug.LogError(\"[{className}] 缺少 BindData 组件\");");
                sb.AppendLine("            return;");
                sb.AppendLine("        }");
                sb.AppendLine();

                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding == null) continue;

                    string typeName = binding.GetType().Name;
                    string goName = binding.gameObject.name;
                    string fieldName = UIBindHierarchyGUI.GenerateFieldName(goName, typeName);

                    // Transform/RectTransform 获取 GameObject
                    if (typeName == "Transform" || typeName == "RectTransform")
                    {
                        sb.AppendLine($"        {fieldName} = bindData.GetGameObject({i});");
                    }
                    else
                    {
                        sb.AppendLine($"        {fieldName} = bindData.Get<{typeName}>({i});");

                        // PointerEventListener 类型额外调用绑定方法
                        if (typeName == "PointerEventListener")
                        {
                            sb.AppendLine($"        PointerEventListenerBind({fieldName});");
                        }
                    }
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    #endregion");

            // 类结束
            sb.AppendLine("}");

            // 命名空间结束
            if (!string.IsNullOrEmpty(ns))
            {
                sb.AppendLine();
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
