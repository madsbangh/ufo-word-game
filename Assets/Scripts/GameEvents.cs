using System;

public static class GameEvents
{
    public static event Action NPCHoisted;

    public static void NotifyNPCHoisted()
    {
        NPCHoisted?.Invoke();
    }
}