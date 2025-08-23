using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject VoiceIcon;

    public TMPro.TextMeshProUGUI TxtChat;
    public TMPro.TextMeshProUGUI TxtHint;

    public Button BtnConfirm;
    public Button BtnRetry;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleVoiceButton(bool state, string hint)
    {
        if (VoiceIcon != null)
        {
            VoiceIcon.SetActive(state);
            TxtChat.text = ""; // Clear chat text when toggling voice button
            TxtHint.text = hint;
            BtnConfirm.gameObject.SetActive(false); // Hide confirm button when toggling voice button
            BtnRetry.gameObject.SetActive(false); // Hide retry button when toggling voice button
        }
        else
        {
            Debug.LogWarning("VoiceIcon is not assigned in the UIManager.");
        }
    }

    public void SetChatText(string text, bool isComplete)
    {
        if (TxtChat != null)
        {
            TxtChat.text = text;
            if (isComplete)
            {
                if (text.Trim().Length > 1)
                {
                    BtnConfirm.gameObject.SetActive(true);
                }
                BtnRetry.gameObject.SetActive(true); // Hide retry button when setting chat text
            } else
            {
                BtnConfirm.gameObject.SetActive(false);
                BtnRetry.gameObject.SetActive(false); // Hide retry button when setting chat texts
            }
        }
        else
        {
            Debug.LogWarning("TxtChat is not assigned in the UIManager.");
        }
    }

    public void AcceptVoice()
    {
        SpeechManager.Instance.AcceptSpeech();
    }

    public void RetryVoice()
    {
        SpeechManager.Instance.RetrySpeech();
    }
}
