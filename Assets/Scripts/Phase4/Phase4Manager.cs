using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Phase4Manager : MonoBehaviour
{
    [SerializeField] private LLMConnectorWinOrLoseSentence _llmConnectorWinOrLoseSentence;
    [SerializeField] private LLMConnectorTextSentence _llmConnectorTextSentence;
    [SerializeField] private GameObject _sentenceHolder;
    [SerializeField] private TMP_Text _loadingSentenceText;
    [SerializeField] private TMP_Text _sentenceText;
    [SerializeField] private GameObject _buttonGoBackToMainMenu;
    [SerializeField] private GameObject _victoryText;
    [SerializeField] private GameObject _defeatText;

    bool _loadingSentence;
    bool _playerWin;

    public void GoBackToMainMenu()
    {
        GameSystem.Instance.ResetCaseData();
        SceneSystem.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Recibe la respuesta de texto de la sentencia
    /// </summary>
    /// <param name="sentenceText"></param>
    private void onRecieveStringAnswer(string sentenceText)
    {
        _loadingSentence = false;
        _loadingSentenceText.gameObject.SetActive(false);

        if (_playerWin)
            _victoryText.SetActive(true);
        else
            _defeatText.SetActive(true);

        _sentenceHolder.SetActive(true);
        _sentenceText.text = sentenceText;
        _buttonGoBackToMainMenu.SetActive(true);
    }

    /// <summary>
    /// Recibir la respuesta del prompt booleano de victoria o derrota
    /// </summary>
    /// <param name="playerWin"></param>
    private void onRecieveBoolAnswer(bool playerWin)
    {
        _playerWin = playerWin;
        _llmConnectorTextSentence.SendPrompt(onRecieveStringAnswer, onRecieveError, playerWin);
    }

    private void onRecieveError(string text)
    {
        _sentenceHolder.SetActive(true);
        _sentenceText.text = "Error del servidor: " + text;
        _buttonGoBackToMainMenu.SetActive(true);
    }

    private IEnumerator animateLoadingText()
    {
        float timer = 0;
        while (_loadingSentence)
        {
            _loadingSentenceText.text = "Generando sentencia";

            int dots = 1 + ((int)timer % 3);
            for (int i = 0; i < dots; i++)
                _loadingSentenceText.text += ".";

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void Start()
    {
        _loadingSentence = true;
        _llmConnectorWinOrLoseSentence.SendPrompt(onRecieveBoolAnswer, onRecieveError);
        StartCoroutine(animateLoadingText());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
