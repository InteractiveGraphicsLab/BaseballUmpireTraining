using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InfiniteScroll))]
public class FilterableListController<T> : UIBehaviour, IInfiniteScrollSetup where T : new() {
    [SerializeField]
    int verticalItemSize = 1000;
    [SerializeField]
    int horizontalItemSize = 1;

    InfiniteScroll iScroll;
    RectTransform rt;
    bool isLoading = true;

    protected ListFilter<T> list;
    protected List<FilterRequirement> conditions;

    protected class FilterRequirement : IFilterRequirement<T> {
        Func<T, bool> predicate;

        public FilterRequirement(Func<T, bool> pred) {
            predicate = pred;
        }

        public bool IsFilterd(T t) => predicate(t);
    }

    public virtual void UpdateList(IReadOnlyList<T> newList) {
        list = new ListFilter<T>(newList);
        foreach (var c in conditions)
            list.conditions.Add(c);
        Filter();
    }

    void ResizeContents(int count) {
        var delta = rt.sizeDelta;
        delta.y = iScroll.itemScale * count;
        rt.sizeDelta = delta;
    }

    protected void OnLoadingStart() {
        isLoading = true;
        iScroll.UpdateItems();
    }

    protected void OnLoadingEnd() {
        iScroll.ResetScrollView();
        ResizeContents(list.data.Count > verticalItemSize ? verticalItemSize : list.data.Count);
        isLoading = false;
        iScroll.UpdateItems();
    }

    public void Filter() {
        list.Filter(OnLoadingStart, OnLoadingEnd);
    }

    public virtual void OnPostSetupItems() {
        iScroll = GetComponent<InfiniteScroll>();
        iScroll.onUpdateItem.AddListener(OnUpdateItem);
        GetComponentInParent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;
        rt = GetComponent<RectTransform>();
        iScroll.UpdateItems();
        conditions = new List<FilterRequirement>();
    }

    public void OnUpdateItem(int itemCount, GameObject obj) {
        if (isLoading || itemCount < 0 || itemCount >= verticalItemSize) {
            obj.SetActive(false);
        } else if (list != null && list.data.Count > 0) {

            obj.SetActive(true);

            var item = obj.GetComponentInChildren<IScrollItem<T>>();
            var arg = new T[horizontalItemSize];
            for (int i = 0; i < horizontalItemSize; i++) {
                if (itemCount * horizontalItemSize + i < list.data.Count)
                    arg[i] = list.data[itemCount * horizontalItemSize + i];
                else
                    arg[i] = new T();
            }
            item.UpdateItem(arg);
        }
    }
}
