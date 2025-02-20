using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ChatController : MonoBehaviour
{
    public GameObject chatMessagePrefab;
    public Transform chatContent;
    public float messageDelay = 1f;
    public float letterDelay = 0.05f;

    public bool disableExitUntilFinished = true;

    private Queue<string> messages = new Queue<string>();
    private bool chatStarted = false;
    private bool isPaused = false;
    private Coroutine chatCoroutine;
    private string currentMessage = "";
    private int letterIndex = 0;
    private TextMeshProUGUI currentTextComponent;

    public string fileName = "ChatScript.txt";

    void Start()
    {
        LoadChatFromFile();
    }

    void LoadChatFromFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_EDITOR || UNITY_STANDALONE
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                messages.Enqueue(line);
            }
        }
        else
        {
            Debug.LogError($"Chat file not found: {filePath}");
        }
#elif UNITY_ANDROID
        StartCoroutine(LoadChatFromAndroid(filePath));
#endif
    }

    IEnumerator LoadChatFromAndroid(string filePath)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string[] lines = www.downloadHandler.text.Split('\n');
                foreach (string line in lines)
                {
                    messages.Enqueue(line.Trim());
                }
            }
            else
            {
                Debug.LogError($"Failed to load chat file on Android: {filePath}");
            }
        }
    }

    public void ToggleChat()
    {
        if (isPaused)
        {
            ResumeChat();
        }
        else
        {
            StartChat();
        }
    }

    public void StartChat()
    {
        if (!chatStarted && messages.Count > 0)
        {
            chatStarted = true;
            if (disableExitUntilFinished)
            {
                FindFirstObjectByType<CameraController>().PreventExit();
            }
            chatCoroutine = StartCoroutine(DisplayMessages());
        }
    }

    public void PauseChat()
    {
        isPaused = true;
    }

    private void ResumeChat()
    {
        isPaused = false;
        if (chatCoroutine == null)
        {
            chatCoroutine = StartCoroutine(DisplayMessages());
        }
    }

    IEnumerator DisplayMessages()
    {
        while (messages.Count > 0)
        {
            if (isPaused)
            {
                yield return new WaitUntil(() => !isPaused);
            }

            currentMessage = messages.Dequeue();
            letterIndex = 0;

            yield return StartCoroutine(TypeMessage(currentMessage));
            yield return new WaitForSeconds(messageDelay);
        }

        if (disableExitUntilFinished)
        {
            FindFirstObjectByType<CameraController>().AllowExit();
        }
    }

    IEnumerator TypeMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        newMessage.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 50);
        newMessage.SetActive(true);
        currentTextComponent = newMessage.GetComponent<TextMeshProUGUI>();
        currentTextComponent.text = "";

        while (letterIndex < message.Length)
        {
            if (isPaused)
            {
                yield return new WaitUntil(() => !isPaused);
            }

            currentTextComponent.text += message[letterIndex];
            letterIndex++;

            yield return new WaitForSeconds(letterDelay);
        }
    }
}
