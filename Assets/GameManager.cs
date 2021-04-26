using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// used to track game states
public enum GameState
{
    Ready,
    Starting,
    Playing,
    Over
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject playerFollower;

    [SerializeField] private Image screenFader;

    private GameState gameState;
    [SerializeField] private float delay = 2f;

    private float temp;
    [SerializeField] TextMeshProUGUI tempText;

    [SerializeField] private GameObject playerPreFab;
    private Entity playerEntity;

    private EnemySpawner enemySpawner;

    private EntityManager entityManager;

    private BlobAssetStore blobAssetStore;

    public PlayerManager PlayerManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        enemySpawner = FindObjectOfType<EnemySpawner>();

         // each World has one EntityManager; store a reference to it
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();

        // settings used to convert GameObject prefab
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);

        // convert the GameObject prefab into an Entity prefab and store it
        Entity playerEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(playerPreFab, settings);
        playerEntity = entityManager.Instantiate(playerEntityPrefab);

        FollowPlayer followPlayer = playerFollower.GetComponent<FollowPlayer>();
        followPlayer.playerEntity = playerEntity;

        PlayerManager = playerFollower.GetComponent<PlayerManager>();        

        gameState = GameState.Ready;

    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.Starting;     
        StartCoroutine(MainGameLoopRoutine());        
    }

    void resetPlayer()
    {
        Debug.Log("reseting player");
        entityManager.SetComponentData(playerEntity, new Translation { Value = new float3(0.0f, 2.1f, 0.0f) });
        entityManager.SetComponentData(playerEntity, new Temperature{Rate=0f, Value=200.6f});
        //UpdateTemperatue(98.6f);
    }

    
    private IEnumerator MainGameLoopRoutine()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(EndGameRoutine());
    }

    
    private IEnumerator StartGameRoutine()
    {
        resetPlayer();

        enemySpawner.Spawn(100);

        screenFader?.CrossFadeAlpha(0f, delay, true);

        yield return new WaitForSeconds(delay);
        gameState = GameState.Playing;
    }
    
    private IEnumerator PlayGameRoutine()
    {
        while (gameState == GameState.Playing)
        {
            yield return null;
        }
    }

    private IEnumerator EndGameRoutine()
    {
        // fade to black and wait
        screenFader?.CrossFadeAlpha(1f, delay, true);
        yield return new WaitForSeconds(delay);

        gameState = GameState.Ready;

        // restart the game
        Start();
    }

    public static void UpdateTemperatue(float temp){

        Instance.temp = temp;
        Instance.tempText.text = ""+Instance.temp;
    }

    private void OnDestroy()
    {
        // Dispose of the BlobAssetStore, else we're get a message:
        // A Native Collection has not been disposed, resulting in a memory leak.
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
        Debug.Log("DESTROY");
    }

    
    // is the game over?
    public static bool IsGameOver()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        return (Instance.gameState == GameState.Over);
    }

    
    // end the game (player has died)
    public static void EndGame()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        Instance.gameState = GameState.Over;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
