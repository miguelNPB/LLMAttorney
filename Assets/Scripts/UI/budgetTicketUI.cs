using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using System.Drawing;

public class BudgetTicketUI : MonoBehaviour
{
    [Header("Etiqueta de Presupuesto")]
    [Tooltip("Texto de presupuesto inicial")]
    public TMP_Text startingBudgetText;
    [Tooltip("Texto de presupuesto actual")]
    public TMP_Text currentBudgetText;
    [Header("Lista de Gastos")]
    [Tooltip("Lista de gastos (necesita VertLayGrp y ContentSizeFitter)")]
    public RectTransform expensesListContainer;
    [Tooltip("Prefab de entrada de gasto (Dos TMP_Text hijos: título y cantidad)")]
    public GameObject expenseEntryPrefab;
    [Header("Formato de moneda")]
    [Tooltip("Formato de moneda para la visualización")]
    public string currencyFormat = "F0";

    private void OnEnable()
    {
        if (BudgetManager.Instance != null)
        {
            BudgetManager.Instance.OnBudgetChanged += Refresh;
            Refresh();
        }
        else
        {
            Debug.LogWarning("[BudgetTicketUI] BudgetManager.Instance no encontrado! Asegúrate de que BudgetManager está en la escena.");
        }
    }

    private void OnDisable()
    {
        if (BudgetManager.Instance != null)
            BudgetManager.Instance.OnBudgetChanged -= Refresh;
    }

    public void Refresh()
    {
        BudgetManager bm = BudgetManager.Instance;
        if (bm == null) return;
 
        if (startingBudgetText != null)
            startingBudgetText.text = $"Starting Budget:  {bm.startingBudget.ToString(currencyFormat)}";
 
        if (expensesListContainer != null)
        {
            foreach (Transform child in expensesListContainer)
                Destroy(child.gameObject);
 
            foreach (ExpenseEntry entry in bm.Expenses)
                SpawnRow(entry.title, entry.amount);
        }
 
        if (currentBudgetText != null)
        {
            currentBudgetText.text = $"REMAINING:  {bm.CurrentBudget.ToString(currencyFormat)}";
 
            currentBudgetText.color = bm.CurrentBudget < 0f ? Color.red : Color.white;
        }
    }

    private void SpawnRow(string title, float amount)
    {
        if (expenseEntryPrefab == null || expensesListContainer == null) return;


        GameObject row = Instantiate(expenseEntryPrefab, expensesListContainer);
 
        TMP_Text titleLabel  = row.transform.Find("TitleLabel")?.GetComponent<TMP_Text>();
        TMP_Text amountLabel = row.transform.Find("AmountLabel")?.GetComponent<TMP_Text>();
 
        if (titleLabel  != null) titleLabel.text  = $"Coste de {title}";
        if (amountLabel != null) amountLabel.text = $"-{amount.ToString(currencyFormat)}";
    }


}
