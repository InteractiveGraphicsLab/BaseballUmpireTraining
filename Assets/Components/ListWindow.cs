using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ListWindow : UIBehaviour {
    protected static List<ListWindow> windows;
    public static int windowNum => windows?.Count ?? 0;

    [SerializeField]
    protected LoadingSlider loadingSlider;

    public static ListWindow GetWindow(string name) {
        return windows?.Find(w => w.gameObject.name == name) ?? throw new System.ArgumentException($"Window named {name} does not exist.", nameof(name));
    }

    public virtual void UpdateList<T>(T param) { }

    public virtual void Awake() {
        if (windows == null) windows = new List<ListWindow>();
        if (!windows.Contains(this)) windows.Add(this);
        if (loadingSlider == null) loadingSlider = GetComponentInChildren<LoadingSlider>();
    }

    public virtual void OnDestroy() {
        windows.Remove(this);
    }
}
