using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class FollowPlayer : MonoBehaviour
{
    public Entity playerEntity;
    public float3 offset;

    public float movementSpeed = 1f;

    private BlobAssetStore blobAssetStore;
    
    private EntityManager entityManager;
    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

    }
    private void OnDestroy()
    {
        // Dispose of the BlobAssetStore, else we're get a message:
        // A Native Collection has not been disposed, resulting in a memory leak.
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(playerEntity != Entity.Null){

            var translation = entityManager.GetComponentData<Translation>(playerEntity);

            transform.position = Vector3.Lerp(transform.position, 
                translation.Value + offset, 
                movementSpeed * Time.deltaTime);        
        }
    }
}
