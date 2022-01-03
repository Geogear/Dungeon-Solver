using UnityEngine;

public static class GameController
{
    private static bool _paused = false;
    private static GameObject _pauseMenu;

    public static void PasueOrResume(bool setTimeScale, bool forPauseMenu = false)
    {
        _paused = !_paused;
        if (setTimeScale)
        {
            Time.timeScale = (_paused) ? 0 : 1;
        }
        
        if(forPauseMenu)
        {
            _pauseMenu.SetActive(_paused);
        }
    }

    public static void SetPMObject()
    {
        _pauseMenu = GameObject.FindWithTag("PauseMenu");
        _pauseMenu.SetActive(false);
    }

    public static void CheckForPause()
    {
        
        if(Input.GetButtonDown("Pause"))
        {
            PasueOrResume(true, true);
        }
    }

    public static void QuitForMainMenu()
    {
        Debug.Log("Quit For Main Menu.");
    }

    public static bool IsPaused() => _paused;
}
