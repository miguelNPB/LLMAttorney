using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public class ExpenseTypeToggle
{
    public PromptType type;
    public bool enabled;
}

[Serializable]
public class ExpenseEntry
{
    public string title;
    public float  amount;

    public ExpenseEntry(string title, float amount)
    {
        this.title  = title;
        this.amount = amount;
    }
}

public class BudgetManager : MonoBehaviour
{

    public static BudgetManager Instance { get; private set; }


    [Header("Configuración del presupuesto")]
    [Tooltip("Presupuesto inicial del caso.")]
    public float startingBudget = 1000f;

    [Header("Escena al entrar en quiebra")]
    [Tooltip("Nombre de la escena que se cargará cuando el presupuesto baje de 0. Debe estar en Build Settings.")]
    public string bankruptcySceneName = "GameOver";

    [Header("Tipos de gasto monitorizados")]
    [Tooltip("Activa qué tipos de documento deben registrarse como gastos.")]
    public List<ExpenseTypeToggle> trackedTypes = new List<ExpenseTypeToggle>
    {
        new ExpenseTypeToggle { type = PromptType.Question, enabled = false },
        new ExpenseTypeToggle { type = PromptType.Conversation,  enabled = false },
        new ExpenseTypeToggle { type = PromptType.Perito,   enabled = true  },
        new ExpenseTypeToggle { type = PromptType.Report,  enabled = true  },
        new ExpenseTypeToggle { type = PromptType.Witness,  enabled = true  },
        new ExpenseTypeToggle { type = PromptType.DocAlt,   enabled = true  },
    };


    public float              CurrentBudget { get; private set; }
    public float              TotalExpenses { get; private set; }

    public IReadOnlyList<ExpenseEntry> Expenses => _expenses;
    private readonly List<ExpenseEntry> _expenses = new List<ExpenseEntry>();

    public event Action OnBudgetChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BudgetManager] Se ha destruido una instancia duplicada. " +
                             "Coloca BudgetManager solo en la escena inicial/de arranque.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        CurrentBudget = startingBudget;
        TotalExpenses = 0f;
    }


    public float SetBudget(string dialogText)
    {
        float greatest = FindGreatestNumber(dialogText);

        if (greatest < 0f)
        {
            Debug.LogWarning("[BudgetManager] SetBudget: no se ha encontrado ningún valor numérico en el diálogo.");
            return -1f;
        }

        startingBudget = greatest;
        CurrentBudget  = greatest;
        TotalExpenses  = 0f;
        _expenses.Clear();

        Debug.Log($"[BudgetManager] Presupuesto establecido en {CurrentBudget:F2}");
        OnBudgetChanged?.Invoke();
        return CurrentBudget;
    }

    public float AddExpense(string documentText, PromptType type, string docTitle = null)
    {
        if (!IsTypeTracked(type))
        {
            Debug.Log($"[BudgetManager] AddExpense: '{type}' no está marcado para seguimiento; se omite.");
            return -1f;
        }

        float cost = FindNthNumber(documentText, 2);

        if (cost < 0f)
        {
            Debug.LogWarning($"[BudgetManager] AddExpense: no se ha encontrado un segundo número para el tipo '{type}'.");
            return -1f;
        }

        string title = string.IsNullOrWhiteSpace(docTitle)
            ? FirstLine(documentText)
            : docTitle;

        TotalExpenses += cost;
        CurrentBudget -= cost;
        _expenses.Add(new ExpenseEntry(title, cost));

        Debug.Log($"[BudgetManager] -{cost:F2} ({type} | \"{title}\") | Restante: {CurrentBudget:F2}");

        OnBudgetChanged?.Invoke();
        CheckBankruptcy();
        return cost;
    }

    public void ResetBudget()
    {
        CurrentBudget = startingBudget;
        TotalExpenses = 0f;
        _expenses.Clear();

        Debug.Log($"[BudgetManager] Presupuesto restablecido a {CurrentBudget:F2}");
        OnBudgetChanged?.Invoke();
    }


    private bool IsTypeTracked(PromptType type)
    {
        foreach (ExpenseTypeToggle toggle in trackedTypes)
            if (toggle.type == type) return toggle.enabled;
        return false;
    }

    private static float FindGreatestNumber(string text)
    {
        MatchCollection matches = Regex.Matches(text, @"\d+(?:[.,]\d+)?");
        float greatest = float.MinValue;
        bool  found    = false;

        foreach (Match m in matches)
            if (TryParseNumber(m.Value, out float val) && val > greatest)
            { greatest = val; found = true; }

        return found ? greatest : -1f;
    }

    private static float FindNthNumber(string text, int n)
    {
        MatchCollection matches = Regex.Matches(text, @"\d+(?:[.,]\d+)?");
        int count = 0;

        foreach (Match m in matches)
        {
            count++;
            if (count == n && TryParseNumber(m.Value, out float val))
                return val;
        }

        return -1f;
    }

    private static bool TryParseNumber(string raw, out float result)
    {
        return float.TryParse(raw.Replace(',', '.'),
                              System.Globalization.NumberStyles.Float,
                              System.Globalization.CultureInfo.InvariantCulture,
                              out result);
    }

    private static string FirstLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "—";
        int idx = text.IndexOfAny(new[] { '\n', '\r' });
        return (idx > 0 ? text.Substring(0, idx) : text).Trim();
    }

    private void CheckBankruptcy()
    {
        if (CurrentBudget < 0f)
        {
            Debug.LogWarning($"[BudgetManager] El presupuesto ha quedado en negativo ({CurrentBudget:F2}). " +
                             $"Cargando '{bankruptcySceneName}'.");

            if (!string.IsNullOrEmpty(bankruptcySceneName))
                SceneManager.LoadScene(bankruptcySceneName);
        }
    }
}