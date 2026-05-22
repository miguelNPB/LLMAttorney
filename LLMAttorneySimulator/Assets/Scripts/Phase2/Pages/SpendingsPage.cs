using TMPro;
using UnityEngine;

/// <summary>
/// Pagina para mostrar los gastos efectuados durante el caso
/// </summary>
public class SpendingsPage : IPage
{
    [Header("Etiqueta de Presupuesto")]
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
        if (BudgetSystem.Instance != null)
        {
            BudgetSystem.Instance.OnBudgetChanged += refresh;
            refresh();
        }
        else
        {
            Debug.LogWarning("[BudgetTicketUI] BudgetManager.Instance no encontrado! Asegúrate de que BudgetManager está en la escena.");
        }
    }

    private void OnDisable()
    {
        if (BudgetSystem.Instance != null)
            BudgetSystem.Instance.OnBudgetChanged -= refresh;
    }

    /// <summary>
    /// Refresca los gastos que hay en el budgetsystem
    /// </summary>
    private void refresh()
    {
        BudgetSystem bm = BudgetSystem.Instance;
        if (bm == null) return;

        if (expensesListContainer != null)
        {
            foreach (Transform child in expensesListContainer)
                Destroy(child.gameObject);

            foreach (BudgetSystem.ExpenseEntry entry in bm.Expenses)
                spawnRow(entry.title, entry.amount);
        }

        if (currentBudgetText != null)
        {
            currentBudgetText.text = $"DINERO RESTANTE:  {bm.CurrentBudget.ToString(currencyFormat)}";
        }
    }

    /// <summary>
    /// Instancia una entrada en el menu de gastos
    /// </summary>
    /// <param name="title"></param>
    /// <param name="amount"></param>
    private void spawnRow(string title, float amount)
    {
        if (expenseEntryPrefab == null || expensesListContainer == null) return;


        GameObject row = Instantiate(expenseEntryPrefab, expensesListContainer);

        TMP_Text titleLabel = row.transform.Find("TitleLabel")?.GetComponent<TMP_Text>();
        TMP_Text amountLabel = row.transform.Find("AmountLabel")?.GetComponent<TMP_Text>();

        if (titleLabel != null) titleLabel.text = $"Coste de {title}";
        if (amountLabel != null) amountLabel.text = $"-{amount.ToString(currencyFormat)}";
    }
    public override void Open()
    {
        // activar los gameobject de la pagina
        _computerSystem.ToggleNotification(Page.Spendings, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);
    }
}
