using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public AudioSource weaponFXSource1;
    public AudioSource weaponFXSource2;
    public AudioSource flameThrowerLoop;

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
        if (flameThrowerLoop != null)
        {
            if (!flameThrowerLoop.isPlaying)
            {
                //flameThrowerLoop.pitch = UnityEngine.Random.Range(0.8f, .83f);
                flameThrowerLoop.Play();
            }
        }
        //this is dumb, but... whatever
        alternateWeapon = !alternateWeapon;

        if(alternateWeapon){
           
               
            if (weaponFXSource1 != null){
                if (!weaponFXSource1.isPlaying)
                {
                    weaponFXSource1.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                    weaponFXSource1.Play();
                }
            }

        } else {
            if(weaponFXSource2 != null){
                if (!weaponFXSource2.isPlaying)
                {
                    weaponFXSource2.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                    weaponFXSource2.PlayDelayed(0.2f);
                }
            }
        }

    }
}
