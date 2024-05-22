using System;

public static class GameEvents
{
    public static event Action NpcHoisted;
    public static event Action<string> BoardWordCompleted;
    public static event Action UpgradesMenuOpened;

    public static void NotifyNpcHoisted()
    {
        NpcHoisted?.Invoke();
    }

    public static void NotifyBoardWordCompleted(string word)
    {
        BoardWordCompleted?.Invoke(word);
    }
    
    public static void NotifyUpgradesMenuOpened()
    {
        UpgradesMenuOpened?.Invoke();
    }
}