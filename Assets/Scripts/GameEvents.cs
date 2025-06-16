using System;

public static class GameEvents
{
    public static event Action OnFoodDroppedInPit;

    public static void TriggerFoodDroppedInPit()
    {
        OnFoodDroppedInPit?.Invoke();
    }
}