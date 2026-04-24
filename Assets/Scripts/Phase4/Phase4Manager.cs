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

    private void onRecieveBoolAnswer(bool playerWin)
    {
        _playerWin = playerWin;
        _llmConnectorTextSentence.PromptTextSentence(playerWin, onRecieveStringAnswer);
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
        // TEST DOCS
        List<Document> playerDocuments = new List<Document>();
        List<Document> rivalDocuments = new List<Document>();
        /*
        playerDocuments.Add(new Document(0, DocumentType.Perito, "Perito de daños por humedad", "Se ha realizado una investigacion sobre la fuente de los desperfectos y daños por humedad desde la casa de Pedro. Se ha descubierto que la fuente de las humeaddes es una fuga de un grifo de la casa de arriba, de la de Ana.", true, 50, false, true));
        playerDocuments.Add(new Document(0, DocumentType.ReceiptFacture, "Factura de reparacion", "Factura de reparacion de las humedades. Costo de 1000 euros.", true, 50, false, true));
        playerDocuments.Add(new Document(0, DocumentType.Report, "Conversacion de whatsapp", "Conversacion de whatsapp donde se ve como Ana ignora los mensajes de Pedro.", true, 50, false, true));
        */
        rivalDocuments.Add(new Document(0, DocumentType.Perito, "Perito de investigacion de fuga", "Se ha realizado una investigacion sobre la fuente de los desperfectos y daños por humedad desde la casa de Ana. Se ha concluido que la fuga es de otro piso ya que las tuberías están en perfecto estado.", true, 50, true, true));
        rivalDocuments.Add(new Document(0, DocumentType.ReceiptFacture, "Factura de reparación de tuberías 2009", "Factura de una reparación de las tuberías de la casa de Ana en 2009.", true, 50, true, true));
        GameSystem.Instance.CaseData.SetLawsuitText("DEMANDA\r\nAl tribunal de instancia de LLMAttorneyLandia\r\nDon procurador Alberto Velazquez, en nombre y representación acreditada de  Pedro Muñoz, según designa apud acta que se verificará en el momento procesal oportuno/según poder para pleitos que se acompaña, y bajo la dirección letrada de D./Dª.  Francisco, letrado/a del Iltre. Colegio de LLMAttorneylandia col. núm. 123456, ante el Tribunal, en el procedimiento de Juicio Ordinario nº 357/2026 comparezco y, como mejor proceda en Derecho\r\nDIGO\r\nQue por medio del presente escrito y siguiendo instrucciones expresas de mi mandante, interpongo la siguiente demanda a Ana Pérez:\r\nDemando que en la casa de Pedro Muñoz se han generado unas humedades a causa de una fuga de agua de una tubería de Ana Velazquez, que ha ignorado las advertencias que le han dado para repararla, causando en moho y humedades y en un gasto en repararlas.​\r\nLa presente demanda se basa en los siguientes\r\nHECHOS\r\n1- Un perito experto en tuberías ha declarado que las humedades son causadas por negligencia en reparación de las tuberías de la vivienda superior, la de Ana\r\n2- Varios intentos de prevención por parte de Pedro, que ha intentado advertir a Ana de la fuga, los cuales fueron ignorados​\r\nFUNDAMENTOS DE DERECHO\r\nI.- COMPETENCIA TERRITORIAL: Es competente este Tribunal al que nos dirigimos, de acuerdo con lo dispuesto en el art. 51 de la Ley de Enjuiciamiento Civil.\r\nII.- LEGITIMACIÓN: Tanto actor/a como demandado/a se encuentran legitimados para ser parte y actuar en el presente procedimiento, teniendo en cuenta el contrato suscrito por ambas partes.\r\nIII- PROCEDIMIENTO: El procedimiento a seguir será el del juicio ordinario de acuerdo con lo dispuesto en el artículo 249 de la Ley de Enjuiciamiento Civil.\r\nIV- REQUISITO DE PROCEDIBILIDAD:\r\nPreviamente a la interposición de la presente demanda, se efectuaron diversas gestiones negociadoras con la parte demandada, con el propósito de cumplir con el requisito de procedibilidad y resolver el litigio de manera amistosa y extrajudicial.\r\nDe conformidad con el artículo 5 de la LO 1/2025 y el artículo 399 LEC, en relación con el artículo 264 de la LEC, sin que se lograse un acuerdo definitivo, se intentó:\r\nSe intentó llegar a un acuerdo proponiendo a Ana subsanar los gastos de reparación de Pedro, más 1000 euros de compensación. Ana rechazó esta propuesta.​\r\nV- RESPECTO DEL FONDO DEL ASUNTO: \r\nSon de aplicación en el presente procedimiento los artículos: \r\naa​\r\nVI- COSTAS: \r\nDe conformidad con el art. 394.1 de la LEC procede la imposición de costas a la adversa, al no presentar el asunto dudas de hecho ni de derecho.\r\nEn caso de estimación parcial de la demanda, de conformidad con el art. 394.2 LEC, procede la imposición de costas a la adversa al haberse negado a iniciar actividad negociadora previa tendente a evitar el procedimiento judicial.\r\nEn caso de desestimación de las pretensiones ejercitadas en la presente demanda, en aplicación de lo dispuesto en el artículo 394.1 LEC, no habrá pronunciamiento de costas a favor de la adversa por haber rehusado resolver el conflicto mediante un medio adecuado de solución de controversias.\r\nAl tribunal formulo la siguiente:\r\nPETICIÓN\r\nPido al tribunal que se considere los hechos mencionados, y se imponga una sanción economica a la parte demandada para compensar los daños causados en la casa de Pedro. Además de una compensación economica extra de 1000 euros por las molestias. ​");
        GameSystem.Instance.CaseData.SetSentenceDocuments(playerDocuments, rivalDocuments);
        //



        _loadingSentence = true;
        _llmConnectorWinOrLoseSentence.PromptBoolSentence(onRecieveBoolAnswer);
        StartCoroutine(animateLoadingText());

    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
