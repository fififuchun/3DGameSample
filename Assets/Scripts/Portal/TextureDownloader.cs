using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public static class TextureDownloader
{
    /// <summary>
    /// URL先の画像をTexture2Dとして取得する
    /// </summary>
    /// <param name="url">取得したい画像のURL</param>
    /// <param name="onCompleted">取得したTexture2Dを受け取るコールバック</param>
    /// <param name="onError">エラーメッセージを受け取るコールバック</param>
    /// <returns></returns>
    public static IEnumerator DownloadTexture(
        string url,
        UnityAction<Texture2D> onCompleted,
        UnityAction<string> onError = null)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.error);
            yield break;
        }

        onCompleted?.Invoke(((DownloadHandlerTexture)request.downloadHandler).texture);
    }
}
