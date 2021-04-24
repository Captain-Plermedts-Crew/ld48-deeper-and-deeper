using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class FireTorch : MonoBehaviour
{
    [Header("Specs")]

    // time between shots
    [SerializeField] private float rateOfFire = 0.15f;

    // GameObject prefab 
    [SerializeField] private GameObject bulletPrefab;
    
    
    // where the weapon's bullet appears
    [SerializeField] private Transform torchTransform;

    private EntityManager entityManager;
    // prefab converted into an entityPrefab
    private Entity bulletEntityPrefab;

    // timer until weapon and shoot again
    private float shotTimer;

    // is the fire button held down?
    private bool isFireButtonDown;
    public bool IsFireButtonDown { get { return isFireButtonDown; } set { isFireButtonDown = value; } }

    // Start is called before the first frame update
    void Start()
    {   
        // get reference to current EntityManager
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // create entity prefab from the game object prefab, using default conversion settings
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        bulletEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(bulletPrefab, settings);
    }
    
    
    public virtual void FireBullet()
    {
        // create an entity based on the entity prefab
        Entity bullet = entityManager.Instantiate(bulletEntityPrefab);

        // set it to the muzzle angle and position
        entityManager.SetComponentData(bullet, new Translation { Value = torchTransform.position });
        entityManager.SetComponentData(bullet, new Rotation { Value = torchTransform.rotation });

    }

    // Update is called once per frame
    void Update()
    {
        IsFireButtonDown = Input.GetButton("Fire1");
        // count up to the next time we can shoot
        shotTimer += Time.deltaTime;
        if (shotTimer >= rateOfFire && isFireButtonDown)
        {
            // fire and reset the timer
            FireBullet();
            shotTimer = 0f;
        }
    }
}
