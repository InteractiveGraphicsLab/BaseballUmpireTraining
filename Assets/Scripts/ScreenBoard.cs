using UnityEngine;
using UnityEngine.UI;

public class ScreenBoard : MonoBehaviour {
    [SerializeField] Text mainBoard;
    [SerializeField] Text subBoard;
    [SerializeField] Text infoBoard;

    public void MainText(string text, Color? color = null) {
        mainBoard.text = text;
        mainBoard.color = color ?? Color.white;
    }

    public void SubText(string text, Color? color = null) {
        subBoard.text = text;
        subBoard.color = color ?? Color.white;
    }

    public void InfoText(string text, Color? color = null) {
        infoBoard.text = text;
        infoBoard.color = color ?? Color.white;
    }

    public void Text(string main = null, string sub = null, string info = null, Color? color = null) {
        Clear();
        if (main != null) MainText(main, color);
        if (sub != null) SubText(sub, color);
        if (info != null) InfoText(info, color);
    }

    public void Clear() {
        MainText("");
        SubText("");
        InfoText("");
    }
}
