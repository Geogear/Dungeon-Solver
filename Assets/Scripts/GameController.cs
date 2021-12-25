using UnityEngine;

public static class GameController
{
    private static bool _paused = false;

    public static void PasueOrResume(bool setTimeScale)
    {
        _paused = !_paused;
        if (setTimeScale)
        {
            Time.timeScale = (_paused) ? 0 : 1;
        }       
    }

    public static bool IsPaused() => _paused;
}
