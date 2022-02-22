using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class LoadingSlider : MonoBehaviour {
    [SerializeField] Slider slider;

    public void OnLoadingStart(UnityAction onStart = null) {
        this.gameObject.SetActive(true);
        slider.value = 0;
        onStart?.Invoke();
    }

    public void OnLoadingEnd(UnityAction onEnd = null) {
        this.gameObject.SetActive(false);
        slider.value = 0;
        onEnd?.Invoke();
    }

    public void OnLoading(float v, UnityAction onStart = null, UnityAction onEnd = null) {
        if (v == 0) OnLoadingStart(onStart);
        else if (v == 1) OnLoadingEnd(onEnd);
        slider.value = v;
    }

    public System.IProgress<float> GetProgressHandler(UnityAction onStart = null, UnityAction onEnd = null) {
        return Progress.Create((float v) => OnLoading(v, onStart, onEnd));
    }

    void Start() {
        this.gameObject.SetActive(false);
        slider.value = 0;
    }

    // void Update() {

    // }
}
