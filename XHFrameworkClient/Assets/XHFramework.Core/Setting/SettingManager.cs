using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
namespace XHFramework.Core {

/// <summary>
/// 游戏配置管理器。
/// </summary>
public class SettingManager : ManagerBase
{
    public override int Priority => 150;

    private float _gameSpeedBeforePause = 1f;
    
    public void SetFrameRate(int frame)
    {
        Application.targetFrameRate = frame;
    }
    
    public void SetGameSpeed(float gameSpeed)
    {
        Time.timeScale = gameSpeed;
    }
    
    public void PauseGame()
    {
        if (_gameSpeedBeforePause<=0)
        {
            return;
        }

        _gameSpeedBeforePause = Time.timeScale;
        Time.timeScale = 0;
    }

    /// <summary>
    /// 恢复游戏。
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = _gameSpeedBeforePause;
    }

    /// <summary>
    /// 重置为正常游戏速度。
    /// </summary>
    public void ResetNormalGameSpeed()
    {
        _gameSpeedBeforePause = 1;
        Time.timeScale = 1;
    }
    
    public override void Init()
    {
        ResetNormalGameSpeed();
        SetFrameRate(60);
        Application.runInBackground = true;
    }

    /// <summary>
    /// 游戏配置管理器轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 关闭并清理游戏配置管理器。
    /// </summary>
    public override void Shutdown()
    {
        Save();
    }


    /// <summary>
    /// 保存游戏配置。
    /// </summary>
    /// <returns>是否保存游戏配置成功。</returns>
    public void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 检查是否存在指定游戏配置项。
    /// </summary>
    /// <param name="settingName">要检查游戏配置项的名称。</param>
    /// <returns>指定的游戏配置项是否存在。</returns>
    public bool HasSetting(string settingName)
    {
        return PlayerPrefs.HasKey(settingName);
    }

    /// <summary>
    /// 移除指定游戏配置项。
    /// </summary>
    /// <param name="settingName">要移除游戏配置项的名称。</param>
    /// <returns>是否移除指定游戏配置项成功。</returns>
    public bool RemoveSetting(string settingName)
    {
        if (!PlayerPrefs.HasKey(settingName))
        {
            return false;
        }

        PlayerPrefs.DeleteKey(settingName);
        return true;
    }

    /// <summary>
    /// 清空所有游戏配置项。
    /// </summary>
    public void RemoveAllSettings()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// 从指定游戏配置项中读取布尔值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <returns>读取的布尔值。</returns>
    public bool GetBool(string settingName)
    {
        return PlayerPrefs.GetInt(settingName) != 0;
    }

    /// <summary>
    /// 从指定游戏配置项中读取布尔值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
    /// <returns>读取的布尔值。</returns>
    public bool GetBool(string settingName, bool defaultValue)
    {
        return PlayerPrefs.GetInt(settingName, defaultValue ? 1 : 0) != 0;
    }

    /// <summary>
    /// 向指定游戏配置项写入布尔值。
    /// </summary>
    /// <param name="settingName">要写入游戏配置项的名称。</param>
    /// <param name="value">要写入的布尔值。</param>
    public void SetBool(string settingName, bool value)
    {
        PlayerPrefs.SetInt(settingName, value ? 1 : 0);
    }

    /// <summary>
    /// 从指定游戏配置项中读取整数值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <returns>读取的整数值。</returns>
    public int GetInt(string settingName)
    {
        return PlayerPrefs.GetInt(settingName);
    }

    /// <summary>
    /// 从指定游戏配置项中读取整数值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
    /// <returns>读取的整数值。</returns>
    public int GetInt(string settingName, int defaultValue)
    {
        return PlayerPrefs.GetInt(settingName, defaultValue);
    }

    /// <summary>
    /// 向指定游戏配置项写入整数值。
    /// </summary>
    /// <param name="settingName">要写入游戏配置项的名称。</param>
    /// <param name="value">要写入的整数值。</param>
    public void SetInt(string settingName, int value)
    {
        PlayerPrefs.SetInt(settingName, value);
    }

    /// <summary>
    /// 从指定游戏配置项中读取浮点数值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <returns>读取的浮点数值。</returns>
    public float GetFloat(string settingName)
    {
        return PlayerPrefs.GetFloat(settingName);
    }

    /// <summary>
    /// 从指定游戏配置项中读取浮点数值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
    /// <returns>读取的浮点数值。</returns>
    public float GetFloat(string settingName, float defaultValue)
    {
        return PlayerPrefs.GetFloat(settingName, defaultValue);
    }

    /// <summary>
    /// 向指定游戏配置项写入浮点数值。
    /// </summary>
    /// <param name="settingName">要写入游戏配置项的名称。</param>
    /// <param name="value">要写入的浮点数值。</param>
    public void SetFloat(string settingName, float value)
    {
        PlayerPrefs.SetFloat(settingName, value);
    }

    /// <summary>
    /// 从指定游戏配置项中读取字符串值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <returns>读取的字符串值。</returns>
    public string GetString(string settingName)
    {
        return PlayerPrefs.GetString(settingName);
    }

    /// <summary>
    /// 从指定游戏配置项中读取字符串值。
    /// </summary>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
    /// <returns>读取的字符串值。</returns>
    public string GetString(string settingName, string defaultValue)
    {
        return PlayerPrefs.GetString(settingName, defaultValue);
    }

    /// <summary>
    /// 向指定游戏配置项写入字符串值。
    /// </summary>
    /// <param name="settingName">要写入游戏配置项的名称。</param>
    /// <param name="value">要写入的字符串值。</param>
    public void SetString(string settingName, string value)
    {
        PlayerPrefs.SetString(settingName, value);
    }

    /// <summary>
    /// 从指定游戏配置项中读取对象。
    /// </summary>
    /// <typeparam name="T">要读取对象的类型。</typeparam>
    /// <param name="settingName">要获取游戏配置项的名称。</param>
    /// <returns>读取的对象。</returns>
    public T GetObject<T>(string settingName)
    {
        return JsonConvert.DeserializeObject<T>(settingName);
    }
    
    /// <summary>
    /// 向指定游戏配置项写入对象。
    /// </summary>
    /// <typeparam name="T">要写入对象的类型。</typeparam>
    /// <param name="settingName">要写入游戏配置项的名称。</param>
    /// <param name="obj">要写入的对象。</param>
    public void SetObject<T>(string settingName, T obj)
    {
        PlayerPrefs.SetString(settingName, JsonConvert.SerializeObject(obj));
    }


}

}