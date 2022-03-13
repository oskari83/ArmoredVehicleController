using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{
    // distance from the front of the camera radius collision
    private const float CAMERA_COLLISION_FORWARD_HIT_OFFSET = 0.1f;

    public float set1 = 2f;
    public float set2 = -360f;

    public float lerpSpeed = 15f;
    public float MaxCamDistance = 20f;
    public float MinCamDistance = 2f;
    public float CamZoomSpeed = -10f;
    public float MinAngle = 10f;
    public float MaxAngle = 45f;
    public float RotSpeed = 2f;
    public float Height = 2.5f;

    public float minGunAngle = 5f;
    public float maxGunAngle = 25f;

    public float sniperSpeedH = 2f;
    public float sniperSpeedV = 2f;
    private float yaw = 0f;
    private float pitch = 0f;

    public int MaxDistance = 5000;
    public LayerMask ObstaclesLayer = default;
    public SimpleTankController vehicle;

    public float currentRotX;
    public float currentRotY;
    private float currentDistance;

    public float sniperZoomSpeed = 200f;
    public Camera sniperCamera;
    public Camera ourCamera;
    public GameObject sniperRig;
    public GameObject sniperRigGun;
    public bool inSniperMode = false;
    public GameObject targetPos;
    float _mouseY;
    float _mouseX;
    public Vector3 aimTarget;

    private Vector3 oldPos;
    private float oldGunX;

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ourCamera = gameObject.GetComponent<Camera>();
        transform.position = (vehicle.transform.position + -vehicle.transform.forward) + (Vector3.up * 2);
        //transform.eulerAngles = Vector3.up * vehicle.transform.eulerAngles.y;
        transform.LookAt(vehicle.transform.position + (Vector3.up * Height));
        currentDistance = MinCamDistance + (MaxCamDistance - MinCamDistance) / 2;
    }

    void Update(){
        _mouseY = Input.GetAxis("Mouse Y");
        _mouseX = Input.GetAxis("Mouse X");

        ControlCamera();
        ControlTurret(aimTarget);

        Vector3 _cameraPos = (vehicle.transform.position - (transform.forward * currentDistance)) + (Vector3.up * Height);
        // camera collision
        if (Physics.Linecast(vehicle.transform.position, _cameraPos, out RaycastHit hit, ObstaclesLayer)){
            _cameraPos = (hit.point + transform.forward * CAMERA_COLLISION_FORWARD_HIT_OFFSET);
        }
        transform.position = _cameraPos;
    }

    private void ControlCamera(){
        if(inSniperMode){
            sniperCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sniperZoomSpeed;
        }else{
            currentDistance += Input.GetAxis("Mouse ScrollWheel") * CamZoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, MinCamDistance, MaxCamDistance);
        }

        if(!inSniperMode){
            if(currentDistance < MinCamDistance + 1.5f){
                inSniperMode = true;
                sniperCamera.enabled = true;
                ourCamera.enabled = false;
            }

            /* old working solution
            currentRotX += _mouseX * RotSpeed;
            currentRotY -= _mouseY * RotSpeed;

            currentRotY = Mathf.Clamp(currentRotY, -MinAngle, MaxAngle);
            Vector3 temp = new Vector3(currentRotY, currentRotX);
            transform.eulerAngles = temp;
            
            if (currentRotX > 360) currentRotX -= 360;
            if (currentRotX < 0) currentRotX += 360;
            */ 

            //new and improved
            currentRotX = _mouseX * RotSpeed;
            currentRotY = -(_mouseY * RotSpeed);
            //currentRotY = Mathf.Clamp(currentRotY, -MinAngle, MaxAngle);
            transform.eulerAngles += new Vector3(currentRotY,currentRotX);
            if (currentRotX > 360) currentRotX -= 360;
            if (currentRotX < 0) currentRotX += 360;
            Vector3 clampthis = transform.eulerAngles;
            if(clampthis.x>45f && clampthis.x<300f){ clampthis.x=45f;}
            if(clampthis.x<345f && clampthis.x>300f){ clampthis.x=345f;}
            transform.eulerAngles = clampthis;

            aimTarget = GetTargetPosition();
            sniperRig.transform.LookAt(aimTarget);
            Vector3 _newTurretRotation2 = sniperRig.transform.localEulerAngles;
            _newTurretRotation2.x = 0;
            _newTurretRotation2.z = 0;
            sniperRig.transform.localRotation = Quaternion.Euler(_newTurretRotation2);

            sniperRigGun.transform.LookAt(aimTarget);
            Vector3 _newGunRot = sniperRigGun.transform.localEulerAngles;
            _newGunRot.y = 0;
            _newGunRot.z = 0;
            sniperRigGun.transform.localRotation = Quaternion.Euler(_newGunRot);

            oldPos = aimTarget;
            oldGunX = sniperRigGun.transform.eulerAngles.x;
            //oldRot = sniperRig.transform.localRotation;
        }else{
            //sniperRig.transform.rotation=oldRot;
            aimTarget = GetTargetPosition();
            gameObject.transform.LookAt(aimTarget);

            sniperRig.transform.LookAt(oldPos);
            Vector3 _newTurretRot = sniperRig.transform.localEulerAngles;
            _newTurretRot.x = 0;
            _newTurretRot.z = 0;
            sniperRig.transform.localRotation = Quaternion.Euler(_newTurretRot);

            //Vector3 _newGunRot = sniperRigGun.transform.eulerAngles;
            //_newGunRot.x = oldGunX;
            //_newGunRot.z = 0;
            //_newGunRot.y = 0;
            //sniperRigGun.transform.rotation = Quaternion.Euler(_newGunRot);

            //Vector3 sniperRigRot = sniperRig.transform.localEulerAngles;
            //sniperRigRot.y = oldRot.eulerAngles.y;
            //sniperRig.transform.localRotation = Quaternion.Euler(sniperRigRot);

            ///*
            //NEW MIGHT NOT WORK
            yaw = 0f;
            pitch = 0f;
            yaw = -sniperSpeedH * Input.GetAxis("Mouse Y");
            pitch = sniperSpeedV * Input.GetAxis("Mouse X");
            sniperRig.transform.eulerAngles += new Vector3(0f, pitch, 0f);
            sniperRigGun.transform.eulerAngles += new Vector3(yaw, 0f, 0f);
            //*/

            /*
            //OLD TRYING TO FIX
            yaw = 0f;
            pitch = 0f;
            yaw -= sniperSpeedH * Input.GetAxis("Mouse Y");
            pitch += sniperSpeedV * Input.GetAxis("Mouse X");
            float newYaw = sniperRigGun.transform.eulerAngles.x+yaw;
            if(newYaw>minGunAngle && newYaw<300f){ newYaw=minGunAngle;}
            if(newYaw<335f && newYaw>300f){ newYaw=335f;}
            if(newYaw<-maxGunAngle && newYaw>-120f){ newYaw=-maxGunAngle;}
            sniperRig.transform.eulerAngles = new Vector3(sniperRig.transform.eulerAngles.x, sniperRig.transform.eulerAngles.y+pitch, 0f);
            sniperRigGun.transform.eulerAngles = new Vector3(newYaw, sniperRigGun.transform.eulerAngles.y, 0f);
            */

            //oldRot = sniperRig.transform.localRotation;

            if(sniperCamera.fieldOfView >= 61){
                inSniperMode = false;
                currentDistance = MinCamDistance + 1.5f;
                sniperCamera.fieldOfView = 60.5f;
                ourCamera.enabled = true;
                sniperCamera.enabled = false;
            }

            //aimTarget = GetTargetPosition();
            //gameObject.transform.LookAt(aimTarget);
            //oldPos = aimTarget;
            if(pitch!=0f){
                oldPos = GetTargetPosition();
            }
            oldGunX = sniperRigGun.transform.eulerAngles.x;
        }

        sniperCamera.fieldOfView = Mathf.Clamp(sniperCamera.fieldOfView, 10, 61);
    }

    private void ControlTurret(Vector3 target){
        vehicle.TurretTargetPosition = target;
    }

    public Vector3 GetTargetPosition(){
        Vector3 _pos;
        if(!inSniperMode){
            _pos = Camera.main.transform.position + (Camera.main.transform.forward * 1000);
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit3, MaxDistance, ObstaclesLayer)){
                _pos = hit3.point;
                Debug.DrawLine(Camera.main.transform.position, hit3.point, Color.blue);
            }
        }else{
            _pos = sniperCamera.transform.position + (sniperCamera.transform.forward * 1000);
            if (Physics.Raycast(sniperCamera.transform.position, sniperCamera.transform.forward, out RaycastHit hit2, MaxDistance, ObstaclesLayer)){
                _pos = hit2.point;
            }
        }
        //targetPos.transform.position = _pos;
        return _pos;
    }
}