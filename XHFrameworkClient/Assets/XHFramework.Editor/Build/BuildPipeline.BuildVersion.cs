using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using XHFramework.Core;
namespace XHFramework.Editor {

public partial class BuildPipelineEditor
{
    /// <summary>
    /// 计算下一个版本号
    /// </summary>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="isFullBuild">true=主版本号+1，false=修订号+1</param>
    public static string GetNextVersion(string currentVersion, bool isFullBuild)
    {
        var match = Regex.Match(currentVersion, @"^(\d+)\.(\d+)\.(\d+)$");
        if (!match.Success)
            throw new Exception("版本号格式错误，应为 x.x.x");

        int major = int.Parse(match.Groups[1].Value);
        int minor = int.Parse(match.Groups[2].Value);
        int patch = int.Parse(match.Groups[3].Value);

        if (isFullBuild)
        {
            major++;
            minor = 0; // 全量包时次版本号归零
            patch = 0;
        }
        else
        {
            if (patch >= 99) // 假设修订号最大为99
            {
                patch = 0;
                minor++;
            }
            else
            {
                patch++;
            }
        }

        return $"{major}.{minor}.{patch}";
    }

    /// <summary>
    /// 计算下一个版本号
    /// </summary>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="isFullBuild">true=主版本号+1，false=修订号+1</param>
    public static int GetAndroidVersion(string currentVersion)
    {
        var match = Regex.Match(currentVersion, @"^(\d+)\.(\d+)\.(\d+)$");
        if (!match.Success)
            throw new Exception("版本号格式错误，应为 x.x.x");

        int major = int.Parse(match.Groups[1].Value);
        return major;
    }

  

    /// <summary>
    /// 管理EnableLog宏定义
    /// </summary>
    /// <param name="enableLog">true为添加EnableLog宏，false为移除EnableLog宏</param>
    public static void SetEnableLogSymbol(string  symbol,bool enableLog)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
            .Split(';')
            .ToList();

        if (enableLog)
        {
            // 添加EnableLog宏
            if (!symbols.Contains(symbol))
            {
                symbols.Add(symbol);
                Log.Info("已添加EnableLog宏定义");
            }
            else
            {
                Log.Info("EnableLog宏定义已存在");
            }
        }
        else
        {
            // 移除EnableLog宏
            if (symbols.Remove(symbol))
            {
                Log.Info("已移除EnableLog宏定义");
            }
            else
            {
                Log.Info("EnableLog宏定义不存在，无需移除");
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols));
    }
}

}
