using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(Animator))]
public class NavigationMenu : UIBehaviour, ICancelHandler {
    [SerializeField] int layer = 0;

    bool _isOpen = false;
    public bool isOpen {
        get {
            return _isOpen;
        }
        set {
            if (value) OpenMenu();
            else CloseMenu();
            _isOpen = value;
        }
    }
    public bool isTransition { get; private set; }
    static readonly int paramIsOpen = Animator.StringToHash("isOpenNaviMenu");
    Animator anim;
    GameObject blocker;
    Canvas rootCanvas = null;

    //UnityEngine.UI.Dropdownの実装をパクった
    GameObject CreateBlocker(Canvas rootCanvas) {
        // Create blocker GameObject.
        GameObject blocker = new GameObject("Blocker");

        // Setup blocker RectTransform to cover entire root canvas area.
        RectTransform blockerRect = blocker.AddComponent<RectTransform>();
        blockerRect.SetParent(rootCanvas.transform, false);
        blockerRect.anchorMin = Vector3.zero;
        blockerRect.anchorMax = Vector3.one;
        blockerRect.sizeDelta = Vector2.zero;

        // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
        Canvas blockerCanvas = blocker.AddComponent<Canvas>();
        blockerCanvas.overrideSorting = true;
        Canvas menuCanvas = GetComponent<Canvas>();
        blockerCanvas.sortingLayerID = menuCanvas.sortingLayerID;
        blockerCanvas.sortingOrder = menuCanvas.sortingOrder - 1;

        // // Find the Canvas that this dropdown is a part of
        // Canvas parentCanvas = null;
        // Transform parentTransform = m_Template.parent;
        // while (parentTransform != null) {
        //     parentCanvas = parentTransform.GetComponent<Canvas>();
        //     if (parentCanvas != null)
        //         break;

        //     parentTransform = parentTransform.parent;
        // }

        // // If we have a parent canvas, apply the same raycasters as the parent for consistency.
        // if (parentCanvas != null) {
        //     Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
        //     for (int i = 0; i < components.Length; i++) {
        //         Type raycasterType = components[i].GetType();
        //         if (blocker.GetComponent(raycasterType) == null) {
        //             blocker.AddComponent(raycasterType);
        //         }
        //     }
        // } else {
        //     // Add raycaster since it's needed to block.
        //     GetOrAddComponent<GraphicRaycaster>(blocker);
        // }

        blocker.AddComponent<OVRRaycaster>().sortOrder = menuCanvas.sortingOrder - 1;


        // Add image since it's needed to block, but make it clear.
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = Color.clear;

        // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
        Button blockerButton = blocker.AddComponent<Button>();
        blockerButton.onClick.AddListener(CloseMenu);

        return blocker;
    }

    void OpenMenu() {
        if (isOpen || isTransition) return;

        blocker = CreateBlocker(rootCanvas);

        anim.SetBool(paramIsOpen, true);
        WaitAnimation("Shown").Forget();
    }

    void CloseMenu() {
        if (!isOpen || isTransition) return;

        if (blocker != null)
            Destroy(blocker);
        blocker = null;

        anim.SetBool(paramIsOpen, false);
        WaitAnimation("Hidden").Forget();
    }

    public void OnCancel(BaseEventData eventData) {
        CloseMenu();
    }

    protected override void OnDisable() {
        if (blocker != null)
            Destroy(blocker);
        blocker = null;

        base.OnDisable();
    }

    async UniTask WaitAnimation(string stateName, UnityEngine.Events.UnityAction onCompleted = null) {
        isTransition = true;

        await UniTask.WaitUntil(() => {
            var state = anim.GetCurrentAnimatorStateInfo(layer);
            return state.IsName(stateName) && state.normalizedTime >= 1;
        });

        isTransition = false;

        onCompleted?.Invoke();
    }

    void Start() {
        anim = GetComponent<Animator>();

        Canvas[] list = GetComponentsInParent<Canvas>();
        if (list.Length != 0) {
            rootCanvas = list[list.Length - 1];
            for (int i = 0; i < list.Length; i++) {
                if (list[i].isRootCanvas) {
                    rootCanvas = list[i];
                    break;
                }
            }
        }
    }
}
