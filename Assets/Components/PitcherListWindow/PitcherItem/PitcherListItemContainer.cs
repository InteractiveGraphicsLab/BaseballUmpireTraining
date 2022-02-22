using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PitcherListItemContainer : UIBehaviour, IScrollItem<Pitcher> {
    PitcherListItem[] plil;

    public void UpdateItem(Pitcher[] pl) {
        if (plil == null) plil = GetComponentsInChildren<PitcherListItem>();
        for (int i = 0; i < pl.Length; i++)
            plil[i].UpdateItem(pl[i]);
    }

    // void Start() {
    //     plil = GetComponentsInChildren<PitcherListItem>();
    // }
}
