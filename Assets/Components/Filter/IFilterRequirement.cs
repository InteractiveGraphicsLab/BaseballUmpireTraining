public interface IFilterRequirement<T> {
    bool IsFilterd(T item);
}