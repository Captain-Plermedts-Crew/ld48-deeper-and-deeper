using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 1f;
    Vector3 movement;           //Movement Axis
    public Rigidbody rigidbody;      //Player Rigidbody Component

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementInput();
        HandleRotationInput();
    }

    void HandleMovementInput()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = 0f;
        movement.z = Input.GetAxis("Vertical");
    }

    void HandleRotationInput()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit))
        {
            transform.LookAt(new Vector3(hit.point.x, 
                transform.position.y,
                hit.point.z));
        }
    }

    void FixedUpdate() 
    {
        rigidbody.MovePosition(rigidbody.position + movement * movementSpeed * Time.fixedDeltaTime);
    }
}
