using UnityEngine;

/// <summary>
/// Simple demo wallet so you can test payouts quickly.
/// Replace this with your real money system later.
/// </summary>
public class SimplePlayerWallet : MonoBehaviour
{
    [SerializeField] private int balance = 0;

    public int Balance => balance;

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        balance += amount;
        Debug.Log($"[SimplePlayerWallet] {name} received +{amount}. New balance={balance}");
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (balance < amount) return false;
        balance -= amount;
        Debug.Log($"[SimplePlayerWallet] {name} spent -{amount}. New balance={balance}");
        return true;
    }
}