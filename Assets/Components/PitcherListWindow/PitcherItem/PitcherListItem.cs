using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class PitcherListItem : UIBehaviour {
    class OnSelectedEvent : UnityEvent<Pitcher> { }
    static OnSelectedEvent onSelected;

    public static void AddSelectedListener(UnityAction<Pitcher> action) {
        if (onSelected == null) onSelected = new OnSelectedEvent();
        onSelected?.AddListener(action);
    }

    public static void RemoveSelectedListener(UnityAction<Pitcher> action) {
        onSelected?.RemoveListener(action);
    }

    [SerializeField] Image uiIcon;
    [SerializeField] Text uiTextMain;
    [SerializeField] Text uiTextSub;
    [SerializeField] Button uiButton;

    Pitcher pitcher;

    public void UpdateItem(Pitcher p) {
        pitcher?.CancelLoadingImage();

        if (p.HaveData) {
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
            return;
        }
        pitcher = p;
        uiTextMain.text = p.name;
        uiTextSub.text = p.id.ToString();
        uiIcon.sprite = null;
        UniTask.Void(async () => {
            uiIcon.sprite = await p.GetPlayerImage();
        });

        uiButton.onClick.RemoveAllListeners();
        uiButton.onClick.AddListener(delegate { onSelected?.Invoke(pitcher); });
    }
}
