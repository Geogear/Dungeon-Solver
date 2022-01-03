using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MMMOnPlay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public void MMMOnQuit()
    {
        Application.Quit();
    }
}
