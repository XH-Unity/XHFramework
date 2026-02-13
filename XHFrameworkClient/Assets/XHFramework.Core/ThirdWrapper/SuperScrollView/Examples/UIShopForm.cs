using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;

namespace XHFramework.Core
{
    /*
    ========================================
    示例2: 商城界面 - XGridView + MonoBehaviour 用法

    Prefab 结构:
    - UIShopForm
      - Content
        - LoopGridView
      - BtnClose

    Item Prefab (ShopItem / VipShopItem):
      - ImgIcon
      - TxtName
      - TxtPrice
      - BtnBuy
      - ImgVip (仅 VipShopItem)
    ========================================
    */

    #region 数据定义

    public class ShopItemData
    {
        public int Id;
        public string Name;
        public int Price;
        public bool IsVip;
    }

    #endregion

    #region ItemView 定义

    public class ShopItemView : MonoBehaviour, IXScrollItemView<ShopItemData>
    {

        private int _index;

        public void OnInit(ShopItemData data)
        {
            
        }

        public void OnUpdate(ShopItemData data, int index)
        {
            _index = index;
        }
        
    }

    #endregion

 

    #region UIForm 定义

    public class UIShopForm : UIForm
    {
        private LoopGridView _loopGridView;
        private LoopGridViewWrapper<ShopItemData> _gridViewWrapper;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 初始化 XGridView（4列）
            _gridViewWrapper = new LoopGridViewWrapper<ShopItemData>(_loopGridView);
            _gridViewWrapper.Init<ShopItemView>(
                getPrefabName: (data, index) => data.IsVip ? "VipShopItem" : "ShopItem",
                columnCount: 4
            );
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 加载商城数据
            LoadShopData();
        }

        protected override void OnClose(object userData)
        {
            base.OnClose(userData);

            _gridViewWrapper?.Clear();
        }

        public new void Clear()
        {
            // 释放 XGridView
            _gridViewWrapper?.Release();
            _gridViewWrapper = null;

            _loopGridView = null;

            base.Clear();
        }

        private void LoadShopData()
        {
            var shopData = new List<ShopItemData>
            {
                new ShopItemData { Id = 1, Name = "铁剑", Price = 100, IsVip = false },
                new ShopItemData { Id = 2, Name = "钢剑", Price = 300, IsVip = false },
                new ShopItemData { Id = 3, Name = "秘银剑", Price = 800, IsVip = false },
                new ShopItemData { Id = 4, Name = "VIP专属剑", Price = 500, IsVip = true },
                new ShopItemData { Id = 5, Name = "皮甲", Price = 200, IsVip = false },
                new ShopItemData { Id = 6, Name = "锁子甲", Price = 500, IsVip = false },
                new ShopItemData { Id = 7, Name = "板甲", Price = 1000, IsVip = false },
                new ShopItemData { Id = 8, Name = "VIP专属甲", Price = 800, IsVip = true },
                new ShopItemData { Id = 9, Name = "小血瓶", Price = 10, IsVip = false },
                new ShopItemData { Id = 10, Name = "中血瓶", Price = 30, IsVip = false },
                new ShopItemData { Id = 11, Name = "大血瓶", Price = 80, IsVip = false },
                new ShopItemData { Id = 12, Name = "VIP礼包", Price = 999, IsVip = true },
            };

            _gridViewWrapper.SetData(shopData);
        }
    }

    #endregion
}
