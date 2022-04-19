public interface IScrollItem<T> {
    void UpdateItem(T[] t);
}

public enum ItemState {
    Normal,
    Selected,
    Highlighted
}