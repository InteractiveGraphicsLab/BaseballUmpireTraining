using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class BallListItem : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IScrollItem<Ball> {
    static string selectedId;
    // static string pname = ""; //どうにかしよう
    class OnSelectedEvent : UnityEvent<Ball> { }
    static OnSelectedEvent onSelected;
    static Transform contentsParent;
    static BallListWindow blwindow;

    public static void AddSelectedListener(UnityAction<Ball> action) {
        if (onSelected == null) onSelected = new OnSelectedEvent();
        onSelected?.AddListener(action);
    }

    public static void RemoveSelectedListener(UnityAction<Ball> action) {
        onSelected?.RemoveListener(action);
    }

    [SerializeField] Image uiIcon;
    [SerializeField] Text uiTextMain;
    [SerializeField] Text uiTextSub;
    [SerializeField] Image uiItem;
    [SerializeField] ColorBlock colors;

    string id;
    Ball ball;
    ItemState state = ItemState.Normal;

    public void UpdateItem(Ball[] bs) {
        if (blwindow == null)
            blwindow = GameObject.Find("BallSearchWindow").GetComponent<BallListWindow>();

        var b = bs[0];

        // if (pname == "")
        //     pname = b.pitcherName;
        // else if (pname != b.pitcherName) {
        //     pname = b.pitcherName;
        //     selectedId = "";
        // }

        id = b.id;
        ball = b;
        uiTextMain.text = b.balltype.ToString();
        uiTextSub.text = $"{b.velocity.ToString("F")} km/s - {b.pitcherName}";
        UniTask.Void(async () => {
            uiIcon.sprite = await blwindow.pitcher.GetPlayerImage();
        });
        Init();
    }

    public void Init() {
        if (state != ItemState.Highlighted)
            state = id == selectedId ? ItemState.Selected : ItemState.Normal;
        ColorSelf(state);
    }

    public void Select() {
        selectedId = id;
        state = ItemState.Selected;
        ResetColors();
        ColorSelf(state);
        onSelected?.Invoke(ball);
    }

    public void Highlight(bool isActive) {
        if (state == ItemState.Selected) return;
        state = isActive ? ItemState.Highlighted : ItemState.Normal;
        ColorSelf(state);
    }

    void ResetColors() {
        if (contentsParent == null)
            contentsParent = this.transform.parent;
        BallListItem[] items = contentsParent.GetComponentsInChildren<BallListItem>();
        for (int i = 0; i < items.Length; i++)
            items[i].Init();
    }

    void ColorSelf(ItemState _state) {
        switch (_state) {
            case ItemState.Selected:
                uiItem.color = colors.selectedColor;
                break;
            case ItemState.Highlighted:
                uiItem.color = colors.highlightedColor;
                break;
            case ItemState.Normal:
            default:
                uiItem.color = colors.normalColor;
                break;
        }
    }

    public static void AddSelectedEventListener(UnityAction<Ball> e) {
        if (onSelected == null)
            onSelected = new OnSelectedEvent();
        onSelected.AddListener(e);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Highlight(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        Highlight(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Select();
    }
}
