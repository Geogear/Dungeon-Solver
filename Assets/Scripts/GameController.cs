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
        var text = _pauseMenu.GetComponentInChildren<UnityEngine.UI.Text>();
        text.enabled = true;
        if(text.text != "YOU DIED")
        {
            text.color = new Color(0.7830189f, 0.1735938f, 0.1735938f, 1);
            text.text = "YOU DIED";
        }
        _resumeButton.SetActive(false);
    }

    public static void OnFinish()
    {
        PauseOrResume(false, true);
        var text = _pauseMenu.GetComponentInChildren<UnityEngine.UI.Text>();
        text.enabled = true;
        text.color = new Color(0.3295786f, 0.8490566f, 0.2042542f, 1);
        text.text = "Congrats! You've Solved The Dungeon.";
        _resumeButton.SetActive(false);
    }

    public static bool IsPaused() => _paused;
}
