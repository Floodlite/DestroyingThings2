using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Esper.Freeloader;

public class GameManager : MonoBehaviour
{
    [SerializeField]private int currentPlayerCount = 0;
    [SerializeField] private int maxPlayerCount = 16;
    [SerializeField] private List<GameObject> playerCharacters = new List<GameObject>();
    [SerializeField] private GameObject genericPlayerObject;

    private string titleScene = "Title Screen";
    private string lobbyScene = "Concourse";
    private string loadingScreen = "Loading Screen";
    private string interScene = "Interlude";
    private List<string> areaOneScenes = new List<string>(){"Broody Dunes", "Havoc Highway",};
    private List<string> areaTwoScenes = new List<string>(){"Warped Waters", "Maniac Mangroves",};
    private List<string> areaThreeScenes = new List<string>(){"Brutal Breach", "Triumph",};
    private string endScene = "The Coast";
    [SerializeField] private int currentStage = 0;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        
    }

    public bool AddPlayer()
    {
        if(maxPlayerCount+1 > currentPlayerCount)
        {
            return false;
        }
        playerCharacters.Add(genericPlayerObject);
        maxPlayerCount++;
        return true;
    }

    public void LoadNextScene()
    {
        string nextScene = "";

        currentStage++;
        switch(currentStage)
        {
            case 1:
                nextScene = areaOneScenes[Random.Range(0, areaOneScenes.Count-1)];
                break;
            case 2:
                nextScene = interScene;
                break;
            case 3:
                nextScene = areaTwoScenes[Random.Range(0, areaTwoScenes.Count-1)];
                break;
            case 4:
                nextScene = interScene;
                break;
            case 5:
                nextScene = areaThreeScenes[Random.Range(0, areaThreeScenes.Count-1)];
                break;
            default:
                Debug.LogWarning("Default case triggered. Check if you spelled the scene name right.");
                break;
        }

        Debug.Log("Loading scene (get ready): " + nextScene);
        //SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        LoadingScreen.Instance.Load(nextScene);
    }

    public void LoadFinaleScene(int choice)
    {
        string nextScene = "";
        switch(choice)
        {
            case 0:
                nextScene = "5473111735 5P4C3D 0U7";
                break;
            case 1:
                nextScene = "D1551D3N75 D353r710N";
                break;
            case 2:
                nextScene = "463 M0V3r5 4UD170r1UM";
                break;
            case 3:
                nextScene = "4rCH173C75 4QU3DUC7";
                break;
            default:
                Debug.LogWarning("Default case triggered. Check if you spelled the scene name right.");
                break;
        }

        Debug.Log("Loading scene (godspeed): " + nextScene);
        //SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        LoadingScreen.Instance.Load(nextScene);
    }

    public void LoadEndScene()
    {
        //SceneManager.LoadScene(endScene, LoadSceneMode.Single);
        LoadingScreen.Instance.Load(endScene);
    }

    public List<GameObject> GetPlayerCharacters()
    {
        return playerCharacters;
    }

}
