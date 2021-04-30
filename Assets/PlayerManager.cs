using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public AudioSource weaponFXSource1;
    public AudioSource weaponFXSource2;

    private bool alternateWeapon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public virtual void PlayWeaponFX()
    {
        //this is dumb, but... whatever
        alternateWeapon = !alternateWeapon;

        if(alternateWeapon){
           
               
            if (weaponFXSource1 != null){
                weaponFXSource1.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                weaponFXSource1.Play();
            }

        } else {
            if(weaponFXSource2 != null){
                weaponFXSource2.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                weaponFXSource2.Play();
            }
        }

    }
}
