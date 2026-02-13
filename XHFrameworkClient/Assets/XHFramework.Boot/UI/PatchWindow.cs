using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XHFramework.Core;

namespace XHFramework.Boot
{
    public class PatchWindow : MonoBehaviour
    {
        /// <summary>
        /// 对话框封装类
        /// </summary>
        private class MessageBox
        {
            private GameObject _cloneObject;
            private Text _content;
            private Button _btnOk;
            private System.Action _clickOk;

            public bool ActiveSelf
            {
                get
                {
                    return _cloneObject.activeSelf;
                }
            }

            public void Create(GameObject cloneObject)
            {
                _cloneObject = cloneObject;
                _content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
                _btnOk = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
                _btnOk.onClick.AddListener(OnClickYes);
            }
            public void Show(string content, System.Action clickOK)
            {
                _content.text = content;
                _clickOk = clickOK;
                _cloneObject.SetActive(true);
                _cloneObject.transform.SetAsLastSibling();
            }
            public void Hide()
            {
                _content.text = string.Empty;
                _clickOk = null;
                _cloneObject.SetActive(false);
            }
            private void OnClickYes()
            {
                _clickOk?.Invoke();
                Hide();
            }
        }

        private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

        // UGUI相关
        private GameObject _messageBoxObj;
        private Slider _slider;
        private Text _tips;

        void Awake()
        {
            _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
            _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
            _tips.text = "Initializing the game world !";
            _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
            _messageBoxObj.SetActive(false);
        }

        /// <summary>
        /// 步骤变化事件处理
        /// </summary>
        public void OnStepChange(string tips)
        {
            _tips.text = tips;
            Log.Info(tips);
        }

        /// <summary>
        /// 发现更新文件事件处理
        /// </summary>
        public void OnFoundUpdateFiles(int totalCount, long totalSizeBytes)
        {
            // 直接开始下载，不等待用户点击（按用户要求）
            float sizeMB = totalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");

            OnStepChange($"Found {totalCount} files to update ({totalSizeMB}MB), starting download...");
        }

        /// <summary>
        /// 下载进度事件处理
        /// </summary>
        public void OnDownloadProgress(int currentDownloadCount, int totalDownloadCount, long currentDownloadSizeBytes, long totalDownloadSizeBytes)
        {
            _slider.value = (float)currentDownloadCount / totalDownloadCount;
            string currentSizeMB = (currentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (totalDownloadSizeBytes / 1048576f).ToString("f1");
            _tips.text = $"{currentDownloadCount}/{totalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
        }

        /// <summary>
        /// 错误事件处理
        /// </summary>
        public void OnError(string errorMessage)
        {
            ShowMessageBox(errorMessage, () =>
            {
                // 错误发生时，可以选择重试或退出应用
                Log.Error($"Error occurred: {errorMessage}");
                // 这里可以根据需要添加重试逻辑
            });
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        private void ShowMessageBox(string content, System.Action ok)
        {
            // 尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            // 如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            // 显示对话框
            msgBox.Show(content, ok);
        }
    }
}