using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float orbitSpeed = 45.0f;

    private float horizontalInput;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        horizontalInput = Input.GetAxis("Mouse X");
    }

    public void RotateCamera() {
        this.transform.Rotate(0, horizontalInput * orbitSpeed * Time.deltaTime, 0, Space.Self);
    }

    void LateUpdate()
    {
        RotateCamera();
        if (target != null) this.transform.position = target.position;
    }
}
