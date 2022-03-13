using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{
    // distance from the front of the camera radius collision
    private const float CAMERA_COLLISION_FORWARD_HIT_OFFSET = 0.1f;

    [Header("Target")]
    public SimpleTankController vehicle;

    [Header("Camera settings")]
    public float MaxCamDistance = 20f;
    public float MinCamDistance = 2f;
    public float CamZoomSpeed = -10f;
    public float MinAngle = 10f;
    public float MaxAngle = 45f;
    public float RotSpeed = 2f;
    public float Height = 2.5f;
    public float sniperSpeedY = 0.75f;
    public float sniperSpeedX = 0.75f;
    public float sniperZoomSpeed = 200f;
    public LayerMask ObstaclesLayer = default;

    private Camera sniperCamera;
    [HideInInspector] public Camera ourCamera;

    [Header("Currently in snipermode?")]
    public bool inSniperMode = false;

    private float camRotY = 0f;
    private float camRotX = 0f;
    private int MaxDistance = 5000;
    private float currentRotX;
    private float currentRotY;
    private float currentDistance;
    private float _mouseY;
    private float _mouseX;
    private float _mouseScroll;
    private Vector3 aimTarget;
    private GameObject sniperRig;
    private GameObject sniperRigGun;

    void Start(){
        //set cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //get snipercam from vehicle
        if(sniperCamera==null){
            sniperCamera = vehicle.sniperCam;
        }
        //initial camera position
        ourCamera = gameObject.GetComponent<Camera>();
        sniperRig = vehicle.sniperTurret;
        sniperRigGun = vehicle.sniperGun;
        transform.position = (vehicle.transform.position + -vehicle.transform.forward) + (Vector3.up * 2);
        transform.LookAt(vehicle.transform.position + (Vector3.up * Height));
        currentDistance = MinCamDistance + (MaxCamDistance - MinCamDistance) / 2;
    }

    void Update(){
        _mouseY = Input.GetAxis("Mouse Y");
        _mouseX = Input.GetAxis("Mouse X");
        _mouseScroll = Input.GetAxis("Mouse ScrollWheel");

        ControlCamera();
        ControlTurret(aimTarget);
        //set camera position
        Vector3 _cameraPos = (vehicle.transform.position - (transform.forward * currentDistance)) + (Vector3.up * Height);
        if (Physics.Linecast(vehicle.transform.position, _cameraPos, out RaycastHit hit, ObstaclesLayer)){
            _cameraPos = (hit.point + transform.forward * CAMERA_COLLISION_FORWARD_HIT_OFFSET);
        }
        transform.position = _cameraPos;
    }

    private void ControlCamera(){
        if(!inSniperMode){
            //zoom in or out on tank
            currentDistance += _mouseScroll * CamZoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, MinCamDistance, MaxCamDistance);
            //get and set out rotation
            currentRotX = _mouseX * RotSpeed;   
            currentRotY = -(_mouseY * RotSpeed);
            transform.eulerAngles += new Vector3(currentRotY,currentRotX);
            //clamp to angles, remember to actually make these be variable based
            Vector3 clampthis = transform.eulerAngles;
            if(clampthis.x>MaxAngle && clampthis.x<300f){ clampthis.x=MaxAngle;}
            if(clampthis.x<(360f-MinAngle) && clampthis.x>300f){ clampthis.x=(360f-MinAngle);}
            transform.eulerAngles = clampthis;
            //set our snipercam rotation while we are not in it
            aimTarget = GetTargetPosition();
            //rotate snipercam turret
            sniperRig.transform.LookAt(aimTarget);
            Vector3 _newTurretRotation2 = sniperRig.transform.localEulerAngles;
            _newTurretRotation2.x = 0;
            _newTurretRotation2.z = 0;
            sniperRig.transform.localRotation = Quaternion.Euler(_newTurretRotation2);
            //rotate snipercam gun
            sniperRigGun.transform.LookAt(aimTarget);
            Vector3 _newGunRot = sniperRigGun.transform.localEulerAngles;
            _newGunRot.y = 0;
            _newGunRot.z = 0;
            sniperRigGun.transform.localRotation = Quaternion.Euler(_newGunRot);
            //change to sniperCam
            if(currentDistance < MinCamDistance + 1.5f){
                inSniperMode = true;
                sniperCamera.enabled = true;
                ourCamera.enabled = false;
            }
        }else{
            //zoom in or out
            sniperCamera.fieldOfView -= _mouseScroll * sniperZoomSpeed;
            //make main camera look at target when we are not in sniper mode
            aimTarget = GetTargetPosition();
            gameObject.transform.LookAt(aimTarget);
            //control snipercam
            camRotY = -sniperSpeedY * _mouseY;
            camRotX = sniperSpeedX * _mouseX;
            sniperRig.transform.eulerAngles += new Vector3(0f, camRotX, 0f);
            sniperRigGun.transform.eulerAngles += new Vector3(camRotY, 0f, 0f);
            //get out of snipercam
            if(sniperCamera.fieldOfView >= 61){
                inSniperMode = false;
                currentDistance = MinCamDistance + 1.5f;
                sniperCamera.fieldOfView = 60.5f;
                ourCamera.enabled = true;
                sniperCamera.enabled = false;
            }
        }
        //clamp fov
        sniperCamera.fieldOfView = Mathf.Clamp(sniperCamera.fieldOfView, 10, 61);
    }

    private void ControlTurret(Vector3 target){
        vehicle.TurretTargetPosition = target;
    }

    public Vector3 GetTargetPosition(){
        Vector3 _pos;
        if(!inSniperMode){
            _pos = transform.position + (transform.forward * 1000);
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit3, MaxDistance, ObstaclesLayer)){
                _pos = hit3.point;
                Debug.DrawLine(transform.position, hit3.point, Color.blue);
            }
        }else{
            _pos = sniperCamera.transform.position + (sniperCamera.transform.forward * 1000);
            if (Physics.Raycast(sniperCamera.transform.position, sniperCamera.transform.forward, out RaycastHit hit2, MaxDistance, ObstaclesLayer)){
                _pos = hit2.point;
            }
        }
        return _pos;
    }
}