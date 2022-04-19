using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class Pitcher {
    static string imgUrl = "https://en.wikipedia.org/api/rest_v1/page/summary/";
    static string noimgPath = "BlankProfile";
    static Sprite noimage = null;

    public int id { private set; get; }
    public string firstName { private set; get; }
    public string lastName { private set; get; }
    public string name => $"{firstName} {lastName}";
    public Sprite image { private set; get; }
    CancellationTokenSource cts;
    public bool HaveData => 0 <= id;

    public Pitcher() : this(-1, "", "") { }

    public Pitcher(int _id, string _fname, string _lname) {
        (id, firstName, lastName) = (_id, _fname, _lname);
        cts = new CancellationTokenSource();
    }

    public Pitcher(Pitcher p) {
        (id, firstName, lastName, image) = (p.id, p.firstName, p.lastName, p.image);
        cts = new CancellationTokenSource();
    }

    // Wikipediaからplayer画像を取得
    public async UniTask<Sprite> GetPlayerImage() {
        if (!HaveData)
            throw new System.InvalidOperationException("parameters not found");

        if (image != null) return image;

        var request = UnityWebRequest.Get(imgUrl + $"{firstName}_{lastName}");
        try {
            await request.SendWebRequest()
                         .WithCancellation(cts.Token);
        } catch (UnityWebRequestException ex) {
            return GetNoimage();
        }

        var json = JsonUtility.FromJson<WikipediaJsonResponse>(request.downloadHandler.text);

        var uwr = UnityWebRequestTexture.GetTexture(json.originalimage.source);
        try {
            await uwr.SendWebRequest()
                     .WithCancellation(cts.Token);
        } catch (UnityWebRequestException ex) {
            return GetNoimage();
        }
        image = DownloadHandlerTexture.GetContent(uwr).ClippingSquare().ToSprite();

        return image;
    }

    public static Sprite GetNoimage() {
        if (noimage == null)
            noimage = Resources.Load<Sprite>(noimgPath);
        return noimage;
    }

    public void CancelLoadingImage() {
        cts.Cancel();
    }
}
