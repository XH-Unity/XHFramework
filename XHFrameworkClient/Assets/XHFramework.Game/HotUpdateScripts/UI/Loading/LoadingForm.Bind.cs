// ==========================================================
// 此文件由工具自动生成，请勿手动修改
// 生成时间: 2026-01-11 22:52:13
// ==========================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XHFramework.Core;

namespace XHFramework.Game
{

public partial class LoadingForm
{
    #region 自动绑定字段

    protected Scrollbar _Scrollbar_LoadingScroll;
    protected TextMeshProUGUI _TextMeshProUGUI_LoadingNum;

    #endregion

    #region 自动绑定初始化

    /// <summary>
    /// 自动绑定初始化
    /// </summary>
    protected override void BindInit()
    {
        var bindData = gameObject.GetComponent<BindData>();
        if (bindData == null)
        {
            UnityEngine.Debug.LogError("[LoadingForm] 缺少 BindData 组件");
            return;
        }

        _Scrollbar_LoadingScroll = bindData.Get<Scrollbar>(0);
        _TextMeshProUGUI_LoadingNum = bindData.Get<TextMeshProUGUI>(1);
    }

    #endregion
}

}
