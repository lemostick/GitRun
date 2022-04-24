using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Look : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 25f;
    [SerializeField] private float xRotation;
    [SerializeField] private Transform playerBody;
    [SerializeField] public Image crossHair;
    [SerializeField] private Movement movementScript;
    bool changeCrossColor;
    public float wallRunRotation;
    float targetRot;
    float startRot;


    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.rotation.z;
        targetRot = startRot;
        CursorLock();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        changeCrossColor = movementScript.canHook;
        if (changeCrossColor)
        {
            crossHair.color = new Color32(0, 255, 0, 255);
        }
        else
        {
            crossHair.color = new Color32(255, 0, 0, 255);
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.localRotation = Quaternion.Euler(xRotation, 0, wallRunRotation);
        playerBody.Rotate(Vector3.up * mouseX);
    }

/*    private void ManageRotation()
    {
        float rotatespeed = 5f;
        rot = Mathf.Lerp(transform.localRotation.z, targetRot, Time.deltaTime * targetRot * rotatespeed);

    }*/

    public void SetTargetRotation(float target)
    {
        targetRot = target;
    }

    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void CursorUnlock()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
