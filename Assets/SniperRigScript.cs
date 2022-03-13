using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRigScript : MonoBehaviour
{
    public CameraController cameraController;
    // Update is called once per frame
    void Update(){
        transform.position = cameraController.vehicle.transform.position;
        transform.position = new Vector3(transform.position.x,transform.position.y + 3f, transform.position.z);   
        transform.LookAt(cameraController.aimTarget);
        Vector3 _newGunRot = transform.localEulerAngles;
        _newGunRot.y = 0;
            //_newGunRot.x = vehicle.gunGO.transform.localEulerAngles.x;
        _newGunRot.z = 0;
        transform.localRotation = Quaternion.Euler(_newGunRot);
    }
}
