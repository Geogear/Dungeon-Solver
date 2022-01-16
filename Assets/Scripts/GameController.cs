using UnityEngine;

public static class GameController
{
    private static bool _onLoading = false;
    private static bool _paused = false;
    private static GameObject _pauseMenu;
    private static GameObject _resumeButton;

    public static void PauseOrResume(bool setTimeScale, bool forPauseMenu = false)
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
        else
        {
            _onLoading = _paused;
        }
    }

    public static void SetPMObject()
    {
        _pauseMenu = GameObject.FindWithTag("PauseMenu");
        _resumeButton = GameObject.FindGameObjectWithTag("ResumeButton");
        _pauseMenu.SetActive(false);
    }

    public static void CheckForPause()
    {
        if(_onLoading)
        {
            return;
        }
        if(Input.GetButtonDown("Pause"))
        {
            PauseOrResume(true, true);
        }
    }

    public static void QuitForMainMenu()
    {
        Room.ClearData();
        Treasure.ClearData();
        LevelGenerator.ClearLGData();
        _paused = false; Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public static void OnDeath()
    {
        PauseOrResume(false, true);
        _pauseMenu.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        _resumeButton.SetActive(false);
    }

    public static bool IsPaused() => _paused;
}
