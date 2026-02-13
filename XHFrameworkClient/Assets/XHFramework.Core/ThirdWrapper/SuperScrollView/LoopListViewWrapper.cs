using System;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;

namespace XHFramework.Core
{
    /// <summary>
    /// ScrollView Item 视图接口
    /// </summary>
    public interface IXScrollItemView<T>
    {
        void OnInit(T data);
        void OnUpdate(T data, int index);
    }

    /// <summary>
    /// 列表视图封装
    /// </summary>
    public class LoopListViewWrapper<T>
    {
        private LoopListView2 _listView;
        private List<T> _dataList = new List<T>();
        private Func<T, int, string> _getPrefabName;
        private Action<LoopListViewItem2, T, int> _onUpdateItem;
        private Action<LoopListViewItem2, T> _onItemInit;

        public LoopListView2 ListView => _listView;
        public List<T> DataList => _dataList;
        public int Count => _dataList.Count;

        public LoopListViewWrapper(LoopListView2 listView) => _listView = listView;

        /// <summary>
        /// 初始化（使用 MonoBehaviour 方式）
        /// </summary>
        public void Init<TView>(Func<T, int, string> getPrefabName) where TView : MonoBehaviour, IXScrollItemView<T>
        {
            Init(
                getPrefabName,
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
        public void Init(Func<T, int, string> getPrefabName, Action<LoopListViewItem2, T, int> onUpdateItem, Action<LoopListViewItem2, T> onItemInit = null)
        {
            _getPrefabName = getPrefabName;
            _onUpdateItem = onUpdateItem;
            _onItemInit = onItemInit;
            _listView.InitListView(0, OnGetItemByIndex);
        }

        public void SetData(List<T> dataList, bool resetPos = true)
        {
            _dataList = dataList ?? new List<T>();
            _listView.SetListItemCount(_dataList.Count, resetPos);
            _listView.RefreshAllShownItem();
        }

        public void AppendData(T data)
        {
            _dataList.Add(data);
            _listView.SetListItemCount(_dataList.Count, false);
            _listView.RefreshAllShownItem();
        }

        public void PrependData(List<T> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;
            _dataList.InsertRange(0, dataList);
            _listView.SetListItemCount(_dataList.Count, false);
            _listView.RefreshAllShownItem();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _dataList.Count) return;
            _dataList.RemoveAt(index);
            _listView.SetListItemCount(_dataList.Count, false);
            _listView.RefreshAllShownItem();
        }

        public void Clear()
        {
            _dataList.Clear();
            _listView.SetListItemCount(0, true);
        }

        /// <summary>
        /// 释放资源（返还对象池前调用）
        /// </summary>
        public void Release()
        {
            _dataList.Clear();
            _listView.SetListItemCount(0, true);
            _getPrefabName = null;
            _onUpdateItem = null;
            _onItemInit = null;
        }

        public void Refresh()
        {
            _listView.SetListItemCount(_dataList.Count, false);
            _listView.RefreshAllShownItem();
        }

        public void RefreshItem(int index)
        {
            var item = _listView.GetShownItemByItemIndex(index);
            if (item != null && index >= 0 && index < _dataList.Count)
                _onUpdateItem?.Invoke(item, _dataList[index], index);
        }

        public T GetData(int index) => (index >= 0 && index < _dataList.Count) ? _dataList[index] : default;

        public void ScrollTo(int index) => _listView.MovePanelToItemIndex(index, 0);
        public void ScrollToBottom() { if (_dataList.Count > 0) _listView.MovePanelToItemIndex(_dataList.Count - 1, 0); }

        public int FindIndex(Predicate<T> predicate) => _dataList.FindIndex(predicate);

        private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
        {
            if (index < 0 || index >= _dataList.Count) return null;

            var data = _dataList[index];
            var prefabName = _getPrefabName(data, index);
            var item = listView.NewListViewItem(prefabName);
            if (item == null) return null;

            if (!item.IsInitHandlerCalled)
            {
                item.IsInitHandlerCalled = true;
                _onItemInit?.Invoke(item, data);
            }

            _onUpdateItem?.Invoke(item, data, index);
            return item;
        }
    }
}
