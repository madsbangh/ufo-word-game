using System;

public static class GameEvents
{
    public static event Action NPCHoisted;
    public static event Action<string> BoardWordCompleted;

    public static void NotifyNPCHoisted()
    {
        NPCHoisted?.Invoke();
    }

    public static void NotifyBoardWordCompleted(string word)
    {
        BoardWordCompleted?.Invoke(word);
    }
}