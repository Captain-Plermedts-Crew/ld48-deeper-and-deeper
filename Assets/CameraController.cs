using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;
    public Vector3 dummyOffset;
    public float zoomAmount = 0f;
    public float zoomRate = 8f;
    public float MaxToClamp = 10f;
    public float MaxZoom = 10f;
    public float MinZoom = 1f;
    public float sharpness = 0.9f;// This blends the Camera looi target rotation in gradually. Keep sharpness between 0 and 1 - lower values are slower/softer.
    public bool Drunk = true;

    //public float maxZoomDist = 10;
    public float movementSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        //targetOffset = new Vector3(1,1,1);
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse Scroll Wheel Controls Zoom (bounded)
        zoomAmount = Input.GetAxis("Mouse ScrollWheel");
        zoomAmount = Mathf.Clamp(zoomAmount, -MaxToClamp, MaxToClamp);
        dummyOffset = targetOffset;
        targetOffset += new Vector3(0, -zoomAmount * zoomRate, zoomAmount * zoomRate *.7f);
        if(Mathf.Abs(targetOffset.z) <= MinZoom || Mathf.Abs(targetOffset.z) >= MaxZoom) //checks to see if we hit the max
        {
            targetOffset = dummyOffset;
        }


            //        targetOffset += new Vector3(0,zoomRate*(-Input.GetAxis("Mouse ScrollWheel")), zoomRate * Input.GetAxis("Mouse ScrollWheel"));

            MoveCamera();
        if(Input.GetKeyDown(KeyCode.Escape)) //this should probably be in a Game State controller system -- eventually when we have menus it'll pull that up instead.
        {
            Application.Quit();
        }
    }

    void MoveCamera()
    {

        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position,
        target.position + targetOffset,
        movementSpeed * Time.deltaTime);

            //transform.LookAt(target); 

             {
                Transform camera = Camera.main.transform;
                Vector3 toTarget = target.position - camera.position;

                // This constructs a rotation looking in the direction of our target,
                Quaternion targetRotation = Quaternion.LookRotation(toTarget);

                // This blends the target rotation in gradually.
                // Keep sharpness between 0 and 1 - lower values are slower/softer.
                
                camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, sharpness*Time.deltaTime);
                //Debug.Log(sharpness * Time.deltaTime);

                if (!Drunk)
                {
                    camera.rotation = Quaternion.Euler(new Vector3(camera.rotation.eulerAngles.x, 0f, 0f));
                }

                // This gives an "stretchy" damping where it moves fast when far
                // away and slows down as it gets closer. You can also use 
                // Quaternion.RotateTowards() to get a more consistent speed.
            }         

        }

    }
}
