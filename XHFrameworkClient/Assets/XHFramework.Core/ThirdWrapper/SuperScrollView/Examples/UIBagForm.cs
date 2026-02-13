using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;

namespace XHFramework.Core
{
    /*
    ========================================
    示例1: 背包界面 - XListView + MonoBehaviour 用法

    Prefab 结构:
    - UIBagForm
      - Content
        - LoopListView2
      - BtnClose

    Item Prefab (BagItem):
      - ImgIcon
      - TxtName
      - TxtCount
      - ImgQuality
      - BtnUse
    ========================================
    */

    #region 数据定义

    public class BagItemData
    {
        public int Id;
        public string Name;
        public int Count;
        public int Quality; // 1-白 2-绿 3-蓝 4-紫 5-橙
    }

    #endregion

    #region ItemView 定义

    public class BagItemView : MonoBehaviour, IXScrollItemView<BagItemData>
    {
        public void OnInit(BagItemData data)
        {
            
        }

        public void OnUpdate(BagItemData data, int index)
        {
           
        }

        private void OnUseClick()
        {
            
        }
    }

    #endregion

    #region UIForm 定义

    public class UIBagForm : UIForm
    {
        private LoopListView2 _loopListView;
        private LoopListViewWrapper<BagItemData> _listViewWrapper;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            // 初始化 XListView
            _listViewWrapper = new LoopListViewWrapper<BagItemData>(_loopListView);
            _listViewWrapper.Init<BagItemView>((data, index) => "BagItem");
        }
        

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            // 加载背包数据
            LoadBagData();
        }

        protected override void OnClose(object userData)
        {
            base.OnClose(userData);

            _listViewWrapper?.Clear();
        }

        public new void Clear()
        {

            // 释放 XListView
            _listViewWrapper?.Release();
            _listViewWrapper = null;

            _loopListView = null;

            base.Clear();
        }

        private void LoadBagData()
        {
            // 模拟数据
            List<BagItemData>  _bagData = new List<BagItemData>
            {
                new BagItemData { Id = 1001, Name = "小血瓶", Count = 99, Quality = 1 },
                new BagItemData { Id = 1002, Name = "中血瓶", Count = 50, Quality = 2 },
                new BagItemData { Id = 1003, Name = "大血瓶", Count = 20, Quality = 3 },
                new BagItemData { Id = 2001, Name = "铁剑", Count = 1, Quality = 1 },
                new BagItemData { Id = 2002, Name = "精钢剑", Count = 1, Quality = 2 },
                new BagItemData { Id = 2003, Name = "秘银剑", Count = 1, Quality = 3 },
                new BagItemData { Id = 2004, Name = "暗影之刃", Count = 1, Quality = 4 },
                new BagItemData { Id = 2005, Name = "屠龙刀", Count = 1, Quality = 5 },
            };

            _listViewWrapper.SetData(_bagData);
        }
    }

    #endregion
}
