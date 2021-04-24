using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

using Random = Unity.Mathematics.Random;

public class AddTiles : MonoBehaviour
{
    [SerializeField] private GameObject tilePreFab;
    private EntityManager entityManager;
    // Entity prefab
    private Entity tileEntityPrefab;

    private Random r = new Random(123);

    private BlobAssetStore blobAssetStore;
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();

        Debug.Log(tilePreFab);
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, 
            blobAssetStore);
        tileEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(tilePreFab, settings);
        Debug.Log(tileEntityPrefab);
        buildFloor();
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

    private void buildFloor(){
        float x = -100.0f;
        float z = -100.0f;
        
        for (int i = 0; i < 100; i++){
            x += 2.0f;
            z = -100.0f;
            for (int j = 0; j < 100; j++){
                z += 2.0f;
                Entity tile = entityManager.Instantiate(tileEntityPrefab);

                entityManager.SetComponentData(tile, new Translation { 
                    Value = new float3(x, -0.35f, z) 
                });

                entityManager.SetComponentData(tile, new Rotation { 
                    Value = Quaternion.Euler(getNextNinety(), getNextNinety(), getNextNinety()) 
                });
            }            
        }
    }

    private float getNextNinety(){
        int i = r.NextInt(0, 3);
        if(i == 0){
            return 0.0f;
        } else if(i == 1){
            return -90.0f;
        } else {
            return 90.0f;
        }
    }
}
