using UnityEngine;
using UnityEngine.UI;

public class HomeWindow : MonoBehaviour {
    [SerializeField] Button randomBtn;
    [SerializeField] Button searchBtn;
    [SerializeField] Button reviewBtn;

    void Start() {
        randomBtn.onClick.AddListener(() => MyGameManager.inst.nowMode = MyMode.Random);
        searchBtn.onClick.AddListener(() => MyGameManager.inst.nowMode = MyMode.Search);
        reviewBtn.onClick.AddListener(() => MyGameManager.inst.nowMode = MyMode.Review);
    }
}
