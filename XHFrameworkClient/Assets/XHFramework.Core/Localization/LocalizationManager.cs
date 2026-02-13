using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XHFramework.Core {

/// <summary>
/// 本地化管理器
/// </summary>
public class LocalizationManager : ManagerBase
{
    public override int Priority => 120;
    public const string LocalizationLanguageSTKey = "LocalizationLanguage";
    public const string LocalizationLanguageDefaut = "CN";

    /// <summary>
    /// 当前语言
    /// </summary>
    public string _language;

    /// <summary>
    /// 支持的语言列表（按表中字段顺序）
    /// </summary>
    private string[] _languageKeys;

    /// <summary>
    /// 获取本地化文本的委托（参数：id, language，返回：文本）
    /// </summary>
    private Func<int, string, string> _getTextFunc;

    /// <summary>
    /// 是否已加载
    /// </summary>
    private bool _isLoaded;

    public override void Init()
    {
    }

    public override void Shutdown()
    {
        _getTextFunc = null;
        _isLoaded = false;
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 设置支持的语言列表
    /// </summary>
    public void SetLanguageTypes(string[] keys)
    {
        _languageKeys = keys;
    }

    /// <summary>
    /// 设置本地化数据获取委托
    /// </summary>
    /// <param name="getTextFunc">获取文本的委托（参数：id, language，返回：文本）</param>
    public void SetLanguageData(Func<int, string, string> getTextFunc)
    {
        _getTextFunc = getTextFunc;
        _isLoaded = true;
        Log.Info($"本地化数据设置成功，支持语言: {string.Join(", ", _languageKeys)}");
    }

    public void ResetLanguage()
    {
        string language = FW.SettingManager.GetString(LocalizationLanguageSTKey);
        if (language == null || !_languageKeys.Contains(language))
        {
            _language = LocalizationLanguageDefaut;
            FW.SettingManager.SetString(LocalizationLanguageSTKey, _language);
        }
        else
        {
            _language = language;
        }
    }

    /// <summary>
    /// 设置语言
    /// </summary>
    public void SetLanguage(string language)
    {
        if (_language == language)
        {
            return;
        }
        _language = language;
        FW.SettingManager.SetString(LocalizationLanguageSTKey, _language);
        FW.EventManager.Fire(this, LocalizationTextChange.Create());
    }

    /// <summary>
    /// 获取当前语言
    /// </summary>
    public string GetLanguage()
    {
        return _language;
    }

    /// <summary>
    /// 获取支持的语言列表
    /// </summary>
    public string[] GetLanguageKeys()
    {
        return _languageKeys;
    }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    public string GetText(int id)
    {
        if (_getTextFunc == null)
        {
            Log.Warn($"本地化数据未设置");
            return $"[{id}]";
        }

        string text = _getTextFunc(id, _language);
        if (string.IsNullOrEmpty(text))
        {
            Log.Warn($"本地化文本未找到: {id}");
            return $"[{id}]";
        }

        return text;
    }

}

}
