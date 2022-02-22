using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class Request {
    public static async UniTask<UnityWebRequest> Get(string url, System.IProgress<float> progressHandler = null) {
        UnityWebRequest request;

        if (progressHandler == null) {
            request = await UnityWebRequest.Get(url)
                                           .SendWebRequest();
        } else {
            request = await UnityWebRequest.Get(url)
                                           .SendWebRequest()
                                           .ToUniTask(progress: progressHandler);
        }

        return request;
    }
}
