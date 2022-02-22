using System;
using UnityEngine;
using UnityEngine.UI;

public class PitcherListController : FilterableListController<Pitcher> {
    [SerializeField] InputField uiNameField;
    string uiName = "Shohei";
    [SerializeField] InputField uiIdField;
    int uiId = 300000;

    void OnNameChanged(InputField input) {
        uiName = input.text;
        Filter();
    }

    void OnIdChanged(InputField input) {
        uiId = Int32.Parse(input.text);
        Filter();
    }

    public override void OnPostSetupItems() {
        base.OnPostSetupItems();

        uiNameField.onEndEdit.AddListener(delegate { OnNameChanged(uiNameField); });
        uiNameField.text = uiName;
        uiIdField.onEndEdit.AddListener(delegate { OnIdChanged(uiIdField); });
        uiIdField.text = uiId.ToString();

        conditions.Add(new FilterRequirement((Pitcher p) => p.name.Contains(uiName)));
        conditions.Add(new FilterRequirement((Pitcher p) => uiId <= p.id));
    }
}
