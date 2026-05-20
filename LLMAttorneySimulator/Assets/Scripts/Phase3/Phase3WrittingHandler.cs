using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Phase3WrittingHandler : MonoBehaviour
{
    [Header("Text display settings")]
    [SerializeField] private WriteTextSystem _writtingSystem;
    [SerializeField] private GameObject _speakingBubbleGameObject;
    [SerializeField] private Image _speakingBubbleBackground;
    [Tooltip("Texto que indica el nombre de la persona que habla")]
    [SerializeField] private TMP_Text _speakingPersonText;
    [SerializeField] private Color _playerSpeakinBubblegBackgroundColor;
    [SerializeField] private Color _rivalSpeakingBubbleBackgroundColor;
    [SerializeField] private Color _judgeSpeakingBubbleBackgroundColor;



    public void SubscribeAction(Action action)
    {
        _writtingSystem.onFinishTyping += action;
    }
    public void DesuscribeAction(Action action)
    {
        _writtingSystem.onFinishTyping -= action;
    }

    /// <summary>
    /// Activa o desactiva la caja de texto donde escriben los personajes al hablar
    /// </summary>
    /// <param name="on"></param>
    public void ToggleSpeakingBubble(bool on)
    {
        _speakingBubbleGameObject.SetActive(on);
    }

    public IEnumerator SpeakPlayer(string text)
    {
        _speakingPersonText.text = "Jugador:";
        _speakingBubbleBackground.color = _playerSpeakinBubblegBackgroundColor;

        yield return _writtingSystem.TypeText(text);
    }
    public IEnumerator SpeakRival(string text)
    {
        _speakingPersonText.text = "Abogado rival:";
        _speakingBubbleBackground.color = _rivalSpeakingBubbleBackgroundColor;

        yield return _writtingSystem.TypeText(text);
    }
    public IEnumerator SpeakJudge(string text)
    {
        _speakingPersonText.text = "Juez:";
        _speakingBubbleBackground.color = _judgeSpeakingBubbleBackgroundColor;

        yield return _writtingSystem.TypeText(text);
    }
}
