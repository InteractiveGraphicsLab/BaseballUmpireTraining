using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NavigationBtn : MonoBehaviour {
    [SerializeField] NavigationMenu naviMenu;

    void Start() {
        GetComponent<Button>().onClick.AddListener(() => naviMenu.isOpen = !naviMenu.isOpen);
    }

    void Update() {
        // if (!isFocused && !naviMenu.isFocused && OVRInput.GetDown(imputModule.joyPadClickButton)) {
        //     naviMenu.isOpen = false;
        // }
    }
}
