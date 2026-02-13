using System;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;

namespace XHFramework.Core
{
    /// <summary>
    /// 网格视图封装
    /// </summary>
    public class LoopGridViewWrapper<T>
    {
        private LoopGridView _gridView;
        private List<T> _dataList = new List<T>();
        private Func<T, int, string> _getPrefabName;
        private Action<LoopGridViewItem, T, int> _onUpdateItem;
        private Action<LoopGridViewItem, T> _onItemInit;
        private int _columnCount;

        public LoopGridView GridView => _gridView;
        public List<T> DataList => _dataList;
        public int Count => _dataList.Count;

        public LoopGridViewWrapper(LoopGridView gridView) => _gridView = gridView;

        /// <summary>
        /// 初始化（使用 MonoBehaviour 方式）
        /// </summary>
        public void Init<TView>(Func<T, int, string> getPrefabName, int columnCount) where TView : MonoBehaviour, IXScrollItemView<T>
        {
            Init(
                getPrefabName,
                columnCount,
                onUpdateItem: (item, data, index) =>
                {
                    var view = item.gameObject.GetComponent<TView>();
                    view?.OnUpdate(data, index);
                },
                onItemInit: (item, data) =>
                {
                    var view = item.gameObject.GetComponent<TView>();
                    if (view == null)
                    {
                        view = item.gameObject.AddComponent<TView>();
                    }
                    view.OnInit(data);
                }
            );
        }

        /// <summary>
        /// 初始化（使用回调方式）
        /// </summary>
        public void Init(Func<T, int, string> getPrefabName, int columnCount, Action<LoopGridViewItem, T, int> onUpdateItem, Action<LoopGridViewItem, T> onItemInit = null)
        {
            _getPrefabName = getPrefabName;
            _columnCount = columnCount;
            _onUpdateItem = onUpdateItem;
            _onItemInit = onItemInit;
            _gridView.InitGridView(0, OnGetItemByRowColumn, null);
        }

        public void SetData(List<T> dataList, bool resetPos = true)
        {
            _dataList = dataList ?? new List<T>();
            RefreshRowCount(resetPos);
            _gridView.RefreshAllShownItem();
        }

        public void Clear()
        {
            _dataList.Clear();
            _gridView.SetListItemCount(0, true);
        }

        /// <summary>
        /// 释放资源（返还对象池前调用）
        /// </summary>
        public void Release()
        {
            _dataList.Clear();
            _gridView.SetListItemCount(0, true);
            _getPrefabName = null;
            _onUpdateItem = null;
            _onItemInit = null;
        }

        public void Refresh()
        {
            RefreshRowCount(false);
            _gridView.RefreshAllShownItem();
        }

        public T GetData(int index) => (index >= 0 && index < _dataList.Count) ? _dataList[index] : default;

        public void ScrollTo(int index) => _gridView.MovePanelToItemByIndex(index / _columnCount, 0);
        public int FindIndex(Predicate<T> predicate) => _dataList.FindIndex(predicate);

        private void RefreshRowCount(bool resetPos)
        {
            int rowCount = Mathf.CeilToInt((float)_dataList.Count / _columnCount);
            _gridView.SetListItemCount(rowCount, resetPos);
        }

        private LoopGridViewItem OnGetItemByRowColumn(LoopGridView gridView, int itemIndex, int row, int column)
        {
            if (itemIndex < 0 || itemIndex >= _dataList.Count) return null;

            var data = _dataList[itemIndex];
            var prefabName = _getPrefabName(data, itemIndex);
            var item = gridView.NewListViewItem(prefabName);
            if (item == null) return null;

            if (!item.IsInitHandlerCalled)
            {
                item.IsInitHandlerCalled = true;
                _onItemInit?.Invoke(item, data);
            }

            _onUpdateItem?.Invoke(item, data, itemIndex);
            return item;
        }
    }
}
