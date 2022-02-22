using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallListController : FilterableListController<Ball> {
    //speed
    [SerializeField]
    InputField uiMinField, uiMaxField;
    int uiMin, uiMax;
    //type
    [SerializeField]
    Dropdown uiTypeDropdown;
    [SerializeField]
    string none = "None";
    List<string> dropdownContents;
    string uiType;

    void OnMinSpeedChanged(string text) {
        uiMin = Int32.Parse(uiMinField.text);
        Filter();
    }

    void OnMaxSpeedChanged(string text) {
        uiMax = Int32.Parse(uiMaxField.text);
        Filter();
    }

    void OnTypeChanged(Dropdown change) {
        uiType = dropdownContents[change.value];
        Filter();
    }

    public override void OnPostSetupItems() {
        base.OnPostSetupItems();

        uiMinField.onValueChanged.AddListener(OnMinSpeedChanged);
        uiMin = Int32.Parse(uiMinField.text);
        uiMaxField.onValueChanged.AddListener(OnMaxSpeedChanged);
        uiMax = Int32.Parse(uiMaxField.text);

        dropdownContents = new List<string>();
        dropdownContents.Add(none);
        foreach (var t in System.Enum.GetValues(typeof(BallData.Type)))
            dropdownContents.Add(t.ToString());
        uiTypeDropdown.ClearOptions();
        uiTypeDropdown.AddOptions(dropdownContents);
        uiTypeDropdown.value = 0;
        uiTypeDropdown.onValueChanged.AddListener(delegate { OnTypeChanged(uiTypeDropdown); });
        uiType = dropdownContents[uiTypeDropdown.value];

        conditions.Add(new FilterRequirement((Ball b) => uiMin <= b.velocity));
        conditions.Add(new FilterRequirement((Ball b) => uiMax >= b.velocity));
        conditions.Add(new FilterRequirement((Ball b) => !BallData.TryGetType(uiType, out BallData.Type t) || b.balltype == t));
    }
}
