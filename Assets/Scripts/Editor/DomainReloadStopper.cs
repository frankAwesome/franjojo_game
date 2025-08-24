#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class DomainReloadStopper
{
    static DomainReloadStopper()
    {
        AssemblyReloadEvents.beforeAssemblyReload += () =>
        {
            // Find all active recognizers and cancel them cleanly
            foreach (var go in Object.FindObjectsOfType<GameObject>())
            {
                // Only active components on active objects
                if (!go || !go.activeInHierarchy) continue;
                var comps = go.GetComponents<SpeechRecognizer>();
                foreach (var c in comps)
                {
                    if (c != null) c.CancelFromEditorReload();
                }
            }
        };
    }
}
#endif
