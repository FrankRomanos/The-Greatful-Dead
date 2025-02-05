using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }
    private int turnNumber = 1;
    public event EventHandler OnTurnChanged;

    public void NextTurn()
    {
        turnNumber++;
        OnTurnChanged?.Invoke(this,EventArgs.Empty);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem!" + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }
}
