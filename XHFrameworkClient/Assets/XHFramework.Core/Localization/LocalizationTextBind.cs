using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XHFramework.Core {

/// <summary>
/// 本地化文本绑定组件
/// 用于将TextMeshProUGUI或Text与本地化文本数据绑定
/// </summary>
[DisallowMultipleComponent]
public class LocalizationTextBind : MonoBehaviour
{
    /// <summary>
    /// 选中的本地化文本ID
    /// </summary>
    [SerializeField]
    private int _selectedId;

    /// <summary>
    /// 选中的Key
    /// </summary>
    [SerializeField]
    private string _selectedCN;

    /// <summary>
    /// 绑定的TextMeshProUGUI组件
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _tmpComponent;

    /// <summary>
    /// 绑定的Text组件
    /// </summary>
    [SerializeField]
    private Text _textComponent;
    public int SelectedId
    {
        get => _selectedId;
        set => _selectedId = value;
    }

    public TextMeshProUGUI TmpComponent
    {
        get => _tmpComponent;
        set => _tmpComponent = value;
    }

    public Text TextComponent
    {
        get => _textComponent;
        set => _textComponent = value;
    }

    /// <summary>
    /// 获取当前使用的文本内容
    /// </summary>
    public string CurrentText
    {
        get
        {
            if (_tmpComponent != null)
                return _tmpComponent.text;
            if (_textComponent != null)
                return _textComponent.text;
            return string.Empty;
        }
    }
    

    private void Awake()
    {
        if (_tmpComponent == null && _textComponent == null)
        {
            _tmpComponent = GetComponent<TextMeshProUGUI>();
            _textComponent = GetComponent<Text>();
        }
    }

    private void OnEnable()
    {
        // 订阅本地化切换事件
        FW.EventManager.Subscribe(LocalizationTextChange.EventId, OnLocalizationTextChange);
        // 立即应用当前本地化文本和字体
        ApplyLocalization();
    }

    private void OnDisable()
    {
        // 取消订阅
        FW.EventManager.Unsubscribe(LocalizationTextChange.EventId, OnLocalizationTextChange);
    }

    private void OnLocalizationTextChange(object sender, GlobalEventArgs args)
    {
        ApplyLocalization();
    }

    /// <summary>
    /// 应用本地化文本和字体
    /// </summary>
    private void ApplyLocalization()
    {
        SetLocalizationText();
    }

    

    /// <summary>
    /// 设置本地化文本
    /// </summary>
    public void SetLocalizationText(int id, string cn, string text)
    {
        _selectedId = id;
        _selectedCN = cn;

        if (_tmpComponent != null)
        {
            _tmpComponent.text = text;
        }

        if (_textComponent != null)
        {
            _textComponent.text = text;
        }
    }

    public void SetLocalizationText()
    {
        string value = FW.LocalizationManager.GetText(_selectedId);
        if (_tmpComponent != null)
        {
            _tmpComponent.text = value;
        }

        if (_textComponent != null)
        {
            _textComponent.text = value;
        }
    }
}

}
