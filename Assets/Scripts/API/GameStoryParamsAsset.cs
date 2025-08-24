using UnityEngine;

[CreateAssetMenu(menuName = "FraJoJo/Game Story Params Asset")]
public class GameStoryParamsAsset : ScriptableObject
{
    [Header("Fetch Settings")]
    public string baseUrl = "https://franklerk.co.za";
    public int storyId = 1;
    public string bearerToken; // optional for this GET; leave empty if not needed

    [Header("Cached Data")]
    public StoryParamsData data;
    [TextArea(5, 30)] public string sourceJson;

    [Header("Change Detection")]
    public string lastETag;        // if server returns ETag
    public string lastModified;    // if server returns Last-Modified
    public string contentHash;     // SHA256 of sourceJson
    public string lastFetchedAt;   // human readable
}
