using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private int maxLines = 3;
    [SerializeField] private bool useMilliseconds = false;

    private Queue<string> lines = new Queue<string>();

    private void Awake()
    {
        ClearConsole();
    }

    public void SetFeedback(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        string timestamp = useMilliseconds
            ? DateTime.Now.ToString("HH:mm:ss.fff")
            : DateTime.Now.ToString("HH:mm:ss");

        lines.Enqueue($"[{timestamp}] {message}");

        while (lines.Count > maxLines)
        {
            lines.Dequeue();
        }

        if (feedbackText != null)
        {
            feedbackText.text = string.Join("\n", lines);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void ClearConsole()
    {
        lines.Clear();

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
        }
    }
}