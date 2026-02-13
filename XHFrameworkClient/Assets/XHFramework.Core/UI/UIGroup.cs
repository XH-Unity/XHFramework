using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XHFramework.Core {

/// <summary>
/// 界面组
/// </summary>
public class UIGroup
{
    #region 字段与属性
    //unity
    private GameObject _gameObject;
    public GameObject GetGameObject() { return _gameObject; }
    public const int DepthFactor = 10000;

    /// <summary>
    /// 界面组深度
    /// </summary>
    private readonly int _depth;

    /// <summary>
    /// 界面组是否暂停
    /// </summary>
    public bool Pause { get; set; }


    /// <summary>
    /// 界面信息链表
    /// </summary>
    private readonly LinkedList<UIForm> _uiForms;
    
    /// <summary>
    /// 当前界面
    /// </summary>
    public UIForm CurrentUIForm=> _uiForms.First != null ? _uiForms.First.Value : null;

    #endregion

    #region 构造方法

    public UIGroup(int depth, GameObject gameObject)
    {
        Pause = false;
        this._gameObject = gameObject;
        _uiForms = new LinkedList<UIForm>();
        _depth = depth;
        
        Canvas cachedCanvas = gameObject.GetOrAddComponent<Canvas>();
        gameObject.GetOrAddComponent<GraphicRaycaster>();
        cachedCanvas.overrideSorting = true;
        cachedCanvas.sortingOrder = DepthFactor * _depth;

        RectTransform transform = gameObject.GetComponent<RectTransform>();
        transform.anchorMin = Vector2.zero;
        transform.anchorMax = Vector2.one;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = Vector2.zero;
        
        cachedCanvas.overrideSorting = true;
        cachedCanvas.sortingOrder = DepthFactor * depth;
    }

    #endregion


    /// <summary>
    /// 界面组轮询
    /// </summary>
    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        LinkedListNode<UIForm> current = _uiForms.First;
        while (current != null)
        {
            if (current.Value.Paused)
            {
                break;
            }

            LinkedListNode<UIForm> next = current.Next;
            current.Value.UpdateForm(elapseSeconds, realElapseSeconds);
            current = next;
        }
    }

    #region 检查界面

    /// <summary>
    /// 界面组中是否存在界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <returns>界面组中是否存在界面</returns>
    public bool HasUIForm(int serialId)
    {
        foreach (UIForm uiForm in _uiForms)
        {
            if (uiForm.SerialId == serialId)
            {
                return true;
            }
        }

        return false;
    }

  
    /// <summary>
    /// 界面组中是否存在界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>界面组中是否存在界面</returns>
    public bool HasUIForm(string uiFormAssetName)
    {
        foreach (UIForm uiForm in _uiForms)
        {
            if (uiForm.UIFormAssetName == uiFormAssetName)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region 获取界面

    /// <summary>
    /// 从界面组中获取界面
    /// </summary>
    /// <param name="serialId">界面序列编号</param>
    /// <returns>要获取的界面</returns>
    public UIForm GetUIForm(int serialId)
    {
        foreach (UIForm uiForm in _uiForms)
        {
            if (uiForm.SerialId == serialId)
            {
                return uiForm;
            }
        }

        return null;
    }

    /// <summary>
    /// 从界面组中获取界面
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称</param>
    /// <returns>要获取的界面</returns>
    public UIForm GetUIForm(string uiFormAssetName)
    {
        foreach (UIForm uiForm in _uiForms)
        {
            if (uiForm.UIFormAssetName == uiFormAssetName)
            {
                return uiForm;
            }
        }

        return null;
    }

    /// <summary>
    /// 从界面组中获取界面。
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称。</param>
    /// <returns>要获取的界面。</returns>
    public UIForm[] GetUIForms(string uiFormAssetName)
    {
        List<UIForm> uiForms = new List<UIForm>();
        foreach (UIForm uiForm in this._uiForms)
        {
            if (uiForm.UIFormAssetName == uiFormAssetName)
            {
                uiForms.Add(uiForm);
            }
        }

        return uiForms.ToArray();
    }

    /// <summary>
    /// 从界面组中获取所有界面
    /// </summary>
    /// <returns>界面组中的所有界面</returns>
    public UIForm[] GetAllUIForms()
    {
        List<UIForm> uiForms = new List<UIForm>();
        foreach (UIForm uiForm in this._uiForms)
        {
            uiForms.Add(uiForm);
        }

        return uiForms.ToArray();
    }

    #endregion

    #region 增加与移除界面
    

    /// <summary>
    /// 往界面组增加界面
    /// </summary>
    /// <param name="uiForm">要增加的界面</param>
    public void AddUIForm(UIForm uiForm)
    {
        _uiForms.AddFirst(uiForm);
    }

    /// <summary>
    /// 从界面组移除界面。
    /// </summary>
    /// <param name="uiForm">要移除的界面。</param>
    public void RemoveUIForm(UIForm uiForm)
    {
        //遮挡界面
        if (!uiForm.Covered)
        {
            uiForm.Covered = true;
            uiForm.Cover();
        }

        //暂停界面
        if (!uiForm.Paused)
        {
            uiForm.Paused = true;
            uiForm.Pause();
        }

        _uiForms.Remove(uiForm);
    }

    #endregion

    /// <summary>
    /// 激活界面
    /// </summary>
    /// <param name="uiForm">要激活的界面</param>
    /// <param name="userData">用户自定义数据</param>
    public void RefocusUIForm(UIForm uiForm, object userData)
    {
        //将激活的界面放到链表最前
        _uiForms.Remove(uiForm);
        _uiForms.AddFirst(uiForm);
    }

    /// <summary>
    /// 刷新界面组
    /// </summary>
    public void Refresh()
    {
        LinkedListNode<UIForm> current = _uiForms.First;
        bool pause = Pause;
        bool cover = false;
        int depth = _uiForms.Count;
        while (current != null)
        {
            LinkedListNode<UIForm> next = current.Next;

            //改变界面深度
            current.Value.DepthChanged(_depth, depth--);

            //界面组暂停时
            if (pause)
            {
                //遮挡未被遮挡的界面
                if (!current.Value.Covered)
                {
                    current.Value.Covered = true;
                    current.Value.Cover();
                }

                //暂停未被暂停的界面
                if (!current.Value.Paused)
                {
                    current.Value.Paused = true;
                    current.Value.Pause();
                }
            }
            else
            {
                //恢复暂停的界面
                if (current.Value.Paused)
                {
                    current.Value.Paused = false;
                    current.Value.Resume();
                }

                //当前界面需要暂停被遮挡的界面时，暂停后面的界面
                if (current.Value.PauseCoveredUIForm)
                {
                    pause = true;
                }


                if (cover)
                {
                    //遮挡未被遮挡的界面
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.Cover();
                    }
                }
                else
                {
                    //恢复遮挡的界面
                    if (current.Value.Covered)
                    {
                        current.Value.Covered = false;
                        current.Value.Reveal();
                    }

                    cover = true;
                }
            }

            current = next;
        }
    }
}

}