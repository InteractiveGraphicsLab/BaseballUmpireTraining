using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ListFilter<T> where T : new() {
    List<T> rawData, filterdData;
    public IReadOnlyList<T> data {
        get {
            if (filterdData == null)
                return new List<T>().AsReadOnly();
            else
                return filterdData.AsReadOnly();
        }
    }

    public List<IFilterRequirement<T>> conditions;

    public ListFilter(IReadOnlyList<T> list) {
        rawData = new List<T>(list);
        conditions = new List<IFilterRequirement<T>>();
        Filter();
    }

    bool FilterCondition(T t) {
        for (int i = 0; i < conditions.Count; i++)
            if (!conditions[i].IsFilterd(t)) return false;
        return true;
    }

    public void Filter(System.Action onFilterStart = null, System.Action onFilterEnd = null) {
        onFilterStart?.Invoke();

        filterdData?.Clear();
        filterdData = rawData.FindAll(t => FilterCondition(t));

        onFilterEnd?.Invoke();
    }

    public async UniTask FilterAsync(System.Action onFilterStart = null, System.Action onFilterEnd = null) {
        await UniTask.Run(() => Filter(onFilterStart, onFilterEnd));
    }
}
