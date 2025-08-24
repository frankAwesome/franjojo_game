#if UNITY_EDITOR
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class GameStoryParamsFetcherEditor
{
    private const string DefaultAssetPath = "Assets/Resources/GameStoryParams.asset";

    [MenuItem("Tools/FraJoJo/Fetch Game Story Params")]
    public static async void FetchMenu()
    {
        var asset = LoadOrCreateAsset();
        await FetchAndSave(asset, showDialogs: true);
    }

    // Optional: auto-create asset from Inspector
    [MenuItem("Tools/FraJoJo/Create Empty Story Params Asset")]
    public static void CreateEmptyAsset()
    {
        var asset = ScriptableObject.CreateInstance<GameStoryParamsAsset>();
        AssetDatabase.CreateAsset(asset, DefaultAssetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
    }

    public static async Task FetchAndSave(GameStoryParamsAsset asset, bool showDialogs)
    {
        if (asset == null)
        {
            if (showDialogs) EditorUtility.DisplayDialog("FraJoJo", "No asset found.", "OK");
            return;
        }

        string url = $"{asset.baseUrl.TrimEnd('/')}/v1/getGameStorieParams/{asset.storyId}";
        try
        {
            string json;
            string eTag, lastModified;
            (json, eTag, lastModified) = await HttpGet(url, asset.bearerToken, asset.lastETag, asset.lastModified);

            if (json == null)
            {
                if (showDialogs) EditorUtility.DisplayDialog("FraJoJo", "No change (HTTP 304). Using cached data.", "OK");
                return;
            }

            var wrapper = JsonUtility.FromJson<StoryParamsWrapper>(json);
            if (wrapper == null || wrapper.response == null)
                throw new Exception("Response missing 'response' wrapper.");

            var newHash = Sha256(json);
            if (asset.contentHash == newHash)
            {
                if (showDialogs) EditorUtility.DisplayDialog("FraJoJo", "Content unchanged. Cache not updated.", "OK");
                return;
            }

            asset.data = wrapper.response;
            asset.sourceJson = json;
            asset.lastETag = eTag;
            asset.lastModified = lastModified;
            asset.contentHash = newHash;
            asset.lastFetchedAt = DateTime.UtcNow.ToString("u");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            if (showDialogs) EditorUtility.DisplayDialog("FraJoJo", "Story params updated & saved.", "Great!");
            Debug.Log($"[FraJoJo] Saved story '{asset.data.title}' (ID {asset.data.storyId}).");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FraJoJo] Fetch failed: {ex.Message}");
            if (showDialogs) EditorUtility.DisplayDialog("FraJoJo Error", ex.Message, "OK");
        }
    }

    private static GameStoryParamsAsset LoadOrCreateAsset()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameStoryParamsAsset>(DefaultAssetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<GameStoryParamsAsset>();
            AssetDatabase.CreateAsset(asset, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FraJoJo] Created asset at {DefaultAssetPath}");
        }
        return asset;
    }

    private static async Task<(string json, string eTag, string lastModified)> HttpGet(
        string url, string bearer, string prevETag, string prevLastModified)
    {
        using var req = UnityWebRequest.Get(url);

        if (!string.IsNullOrEmpty(bearer))
            req.SetRequestHeader("Authorization", "Bearer " + bearer);

        // Conditional GET: if server supports, you'll get 304 Not Modified
        if (!string.IsNullOrEmpty(prevETag))
            req.SetRequestHeader("If-None-Match", prevETag);
        if (!string.IsNullOrEmpty(prevLastModified))
            req.SetRequestHeader("If-Modified-Since", prevLastModified);

        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        // 304 -> unchanged
        if ((int)req.responseCode == 304)
            return (null, req.GetResponseHeader("ETag"), req.GetResponseHeader("Last-Modified"));

#if UNITY_2020_2_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
            throw new Exception($"HTTP {req.responseCode}: {req.error ?? req.downloadHandler?.text}");

        string etag = req.GetResponseHeader("ETag");
        string lastModified = req.GetResponseHeader("Last-Modified");
        return (req.downloadHandler.text, etag, lastModified);
    }

    private static string Sha256(string s)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
#endif
