using UnityEngine;

public class BudgetCalculator : MonoBehaviour
{

    private int _budget;

    public void addBudget(int budget)
    {
        _budget += budget;
    }

    public int getBudget() { 
        return _budget; 
    }

}
