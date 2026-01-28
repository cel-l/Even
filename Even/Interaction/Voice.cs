using System;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Logger = Even.Utils.Logger;
namespace Even.Interaction;

public class Voice : MonoBehaviour
{
    private KeywordRecognizer _keywordRecognizer;
    private string[] _keywords;

    public event Action<string> PhraseRecognized;
    public event Action<string, DateTime> PhraseRecognizedWithStartTime;

    public string LastResult { get; private set; } = "";
    public bool IsReady { get; private set; }

    public void Initialize(string[] keywords)
    {
        StartListening(keywords);

        IsReady = true;
        Logger.Info("Voice initialized and listening");
    }

    public void StartListening(string[] keywords)
    {
        if (keywords == null || keywords.Length == 0)
        {
            Logger.Error("No keywords provided for recognition");
            return;
        }

        StopListening();

        _keywords = keywords;
        _keywordRecognizer = new KeywordRecognizer(_keywords, ConfidenceLevel.Low);
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();
    }

    public void StopListening()
    {
        if (_keywordRecognizer == null)
            return;

        _keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;

        if (_keywordRecognizer.IsRunning)
            _keywordRecognizer.Stop();

        _keywordRecognizer.Dispose();
        _keywordRecognizer = null;
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        LastResult = args.text;
        Logger.Info($"Recognized keyword: {LastResult}");

        PhraseRecognized?.Invoke(LastResult);
        PhraseRecognizedWithStartTime?.Invoke(LastResult, args.phraseStartTime);
    }

    private void OnDestroy()
    {
        StopListening();
    }

    public void ClearLastResult()
    {
        LastResult = "";
    }
}