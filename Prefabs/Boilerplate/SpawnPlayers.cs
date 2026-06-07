using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] private Dictionary<GameObject, bool> spawnPoints = new Dictionary<GameObject, bool>();
    [SerializeField] private List<GameObject> playerCharacters;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        spawnPoints = SetSpawns(this.transform, "Player Spawn");
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }

    private void Start()
    {
        playerCharacters = gameManager.GetPlayerCharacters();
        PlacePeople();
    }

    private Dictionary<GameObject, bool> SetSpawns(Transform parent, string tag)
    {
        Dictionary<GameObject, bool> points = new Dictionary<GameObject, bool>();
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent.gameObject && childTransform.CompareTag(tag))
            {
                points.Add(childTransform.gameObject, false);
            }
        }
        return points;
    }

    private void PlacePeople()
    {
        foreach(GameObject playerCharacter in playerCharacters)
        {
            SpawnPlayer(playerCharacter);
        }
    }

    private void SpawnPlayer(GameObject playerToSpawn)
    {
        foreach(GameObject key in spawnPoints.Keys.ToList())
        {
            if(spawnPoints[key])
            {
                continue;
            }

            Instantiate(playerToSpawn, key.transform.position + new Vector3(0f, 0f, 0f), key.transform.rotation);

            spawnPoints[key] = true;
        }
    }
}
