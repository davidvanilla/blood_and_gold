using System;
using UnityEngine;

public static class GameEvents
{
    // Define the event (Action is a delegate with no return type)
    // You can use Action<int> if you want to pass data like item ID or count
    public static event Action OnItemPickedUp;

    public static event Action OnEnteredItem;
    public static event Action OnExitedItem;

    public static event Action OnHealthChanged;

    public static event Action<GameObject> DestroyItem;
    public static event Action OnAmoCountChanged;


    public static event Action OnZombieDied;
   

    public static void TriggerZombieDied()
    {
        OnZombieDied?.Invoke();
    }
    public static void TriggerItemPickedUp()
    {
        OnItemPickedUp?.Invoke();
    }

    public static void TriggerOnEneteredItem()
    {
        OnEnteredItem?.Invoke();
    }
    public static void TriggerOnExitedItem()
    {
        OnExitedItem?.Invoke();
    }

    public static void TriggerHealthChanged()
    {
        OnHealthChanged?.Invoke();
    }

    public static void TriggerDestroyItem(GameObject obj)
    {
        DestroyItem?.Invoke(obj);
    }

    public static void TriggerAmoCountChanged()
    {
        OnAmoCountChanged?.Invoke();
    }
}