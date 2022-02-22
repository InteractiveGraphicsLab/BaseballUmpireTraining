using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class CSVDownloadHander : DownloadHandlerScript {
    List<List<string>> _csv;
    public IReadOnlyList<IReadOnlyList<string>> csv {
        get {
            if (_csv == null)
                return new List<List<string>>().AsReadOnly();
            else
                return _csv.AsReadOnly();
        }
    }

    class OnCSVEvent : UnityEvent<IReadOnlyList<IReadOnlyList<string>>> { };
    OnCSVEvent onCompleted;
    string buf = "";
    int offset = 0;
    ulong length = 0;

    public CSVDownloadHander(UnityAction<IReadOnlyList<IReadOnlyList<string>>> onCompletedCallback = null) {
        _csv = new List<List<string>>();
        onCompleted = new OnCSVEvent();
        if (onCompletedCallback != null)
            onCompleted.AddListener(onCompletedCallback);
    }

    protected override string GetText() {
        return buf;
    }

    protected override bool ReceiveData(byte[] data, int dataLength) {
        string csvText = System.Text.Encoding.UTF8.GetString(data);
        offset += dataLength;

        int i = csvText.LastIndexOf('\n');
        CastCSVList(buf + csvText.Substring(0, i)).Forget();
        buf = csvText.Substring(i + 1);
        return true;
    }

    protected override void CompleteContent() {
        Debug.Log($"CompleteContent");
        onCompleted?.Invoke(_csv);
    }

    protected override void ReceiveContentLengthHeader(ulong contentLength) {
        length = contentLength;
    }

    protected override float GetProgress() {
        if (length == 0)
            return 0.0f;

        return (float)offset / length;
    }

    async UniTaskVoid CastCSVList(string csvText) {
        var csvList = await MyCSV.Load(csvText);
        _csv.AddRange(csvList);
    }
}
