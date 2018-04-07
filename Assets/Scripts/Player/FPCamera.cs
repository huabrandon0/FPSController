// Usage: this script is meant to be placed on a Camera that is a child of the player.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCamera : MonoBehaviour
{
    // The values used to set the rotation of the player and camera
    private float xRot;
    private float yRot;
    
    [SerializeField] private Transform playerTransform; // The transform used to rotate around y-axis (should be a parent of the camera)
    [SerializeField] private float minimumPitch = -89f;
    [SerializeField] private float maximumPitch = 89f;
    [SerializeField] private float sensitivity = 1f;

    private void GetInput()
    {
        // Retrieve mouse input
        this.xRot -= Input.GetAxis("Mouse Y") * sensitivity;
        this.yRot += Input.GetAxis("Mouse X") * sensitivity;
    }

    private void Awake()
    {
        if (playerTransform == null)
            Debug.LogError(GetType() + ": The player transform is not initialized.");
    }

    private void Update()
    {
        // Retrieve mouse input
        this.xRot -= Input.GetAxis("Mouse Y") * sensitivity;
        this.yRot += Input.GetAxis("Mouse X") * sensitivity;

        // Keep rotation values within (-180, 180], while effectively maintaining the same rotation value
        this.xRot = Normalize(this.xRot);
        this.yRot = Normalize(this.yRot);

        // Bound pitch rotation
        this.xRot = Mathf.Clamp(this.xRot, this.minimumPitch, this.maximumPitch);

        // Set player rotation around the y-axis, camera rotation around the x-axis
        this.playerTransform.localRotation = Quaternion.Euler(0f, this.yRot, 0f);
        this.transform.localRotation = Quaternion.Euler(this.xRot, 0f, this.transform.localRotation.eulerAngles.z);
    }

    // Returns an equivalent rotation float value in (-180, 180]
    private float Normalize(float f)
    {
        if (f >= 180f)
            return f - 360f;
        else if (f < -180f)
            return f + 360f;
        else
            return f;
    }
}
