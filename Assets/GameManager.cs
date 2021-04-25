using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject playerFollower;

    [SerializeField] TextMeshPro tempText;

    [SerializeField] private GameObject playerPreFab;
    private Entity playerEntity;

    private EntityManager entityManager;

    private BlobAssetStore blobAssetStore;

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

    }

    // Start is called before the first frame update
    void Start()
    {
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


        entityManager.SetComponentData(playerEntity, new Translation { Value = new float3(0.0f, 2.1f, 0.0f) });
        //entityManager.SetComponentData(playerEntity, new Rotation { Value = new quaternion(0, 0, 0, 0) });
        entityManager.SetComponentData(playerEntity, new Temperature{tempLossRate=0f, temperature=98.6f});   
    }

    private void OnDestroy()
    {
        // Dispose of the BlobAssetStore, else we're get a message:
        // A Native Collection has not been disposed, resulting in a memory leak.
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
