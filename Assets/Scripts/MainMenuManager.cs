using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Text _seedText;

    private GameObject _characterSelection;
    private GameObject _mainMenu;
    // Start is called before the first frame update
    void Start()
    {
        _characterSelection = GameObject.FindGameObjectWithTag("CharacterSelection");
        _characterSelection.SetActive(false);
        _mainMenu = GameObject.FindGameObjectWithTag("PauseMenu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MMMOnFakePlay()
    {
        _characterSelection.SetActive(true);
        _mainMenu.SetActive(false);
    }

    public void MMMOnPlay()
    {
        int seed = 0;
        if (int.TryParse(_seedText.text, out seed))
        {
            LevelGenerator.SetSeed(seed);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public void MMMOnQuit()
    {
        Application.Quit();
    }

    public void CharacterSelect1() { CharacterSelect(FAType.FallenAngel1); }
    public void CharacterSelect2() { CharacterSelect(FAType.FallenAngel2); }
    public void CharacterSelect3() { CharacterSelect(FAType.FallenAngel3); }

    private void CharacterSelect(FAType selection)
    {
        PlayerCharacter._FAType = selection;
        MMMOnPlay();
    }
}
