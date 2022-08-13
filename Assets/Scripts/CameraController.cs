using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour{
    [Header("Target")]
    public VehicleController vehicle;

    [Header("Camera settings")]
    public float MaxCamDistance = 20f;
    public float MinCamDistance = 2f;
    public float CamZoomSpeed = -10f;
    public float[] zoomStepsSniperMode = new float[5] {60,45,32,20,10};
    public float[] zoomStepsNormal = new float[8] {20,17.4f,14.8f,12.2f,9.6f,7f,4.4f,1.8f};
    public float MinAngle = 10f;
    public float MaxAngle = 45f;
    public float RotSpeed = 2f;
    public float Height = 2.5f;
    public float sniperSpeedY = 0.75f;
    public float sniperSpeedX = 0.75f;
    public float sniperZoomSpeed = 200f;
    public LayerMask ObstaclesLayer = default;
    public LayerMask StabilizerLayer = default; //nothing in inspector

    private Camera sniperCamera;
    [HideInInspector] public Camera ourCamera;

    [Header("Currently in snipermode?")]
    public bool inSniperMode = false;

    // distance from the front of the camera radius collision
    private const float CAMERA_COLLISION_FORWARD_HIT_OFFSET = 1.0f;

    private float camRotY = 0f;
    private float camRotX = 0f;
    private int MaxDistance = 5000;
    private float currentRotX;
    private float currentRotY;
    private float currentDistance;
    private float _mouseY;
    private float _mouseX;
    private float _mouseScroll;
    private float minGunAngle;
    private float maxGunAngle;
    private Vector3 aimTarget;
    private GameObject sniperRig;
    private GameObject sniperRigGun;
    private Vector3 stabilizerPos;
    private Vector3 stabilizerPos2;
    private int zoomPointer = 0;
    private int zoomPointerNormal = 0;

    private void Start(){
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
        minGunAngle = vehicle.minGunAngle;
        maxGunAngle = vehicle.maxGunAngle;
        transform.position = (vehicle.transform.position + -vehicle.transform.forward) + (Vector3.up * 2);
        transform.LookAt(vehicle.transform.position + (Vector3.up * Height));
        //old
        //currentDistance = MinCamDistance + (MaxCamDistance - MinCamDistance) / 2;
        currentDistance = zoomStepsNormal[zoomPointerNormal];
    }

    private void Update(){
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
        //do camera zoom
        Zoom(inSniperMode);

        if(!inSniperMode){
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
            //rotate the sniperRig turret -> does not affect our camera
            sniperRig.transform.LookAt(aimTarget);
            sniperRig.transform.localRotation = Quaternion.Euler(SniperRigTurretEulerSingleAxis());
            //rotate the sniperRig gun -> does not affect our camera
            sniperRigGun.transform.LookAt(aimTarget);
            sniperRigGun.transform.localRotation = Quaternion.Euler(SniperRigGunEulerSingleAxis());
        }else{
            //make main camera look at target when we are not in sniper mode
            aimTarget = GetTargetPosition();
            gameObject.transform.LookAt(aimTarget);
            //rotate the sniperRig turret -> affects our camera
            sniperRig.transform.LookAt(stabilizerPos);
            sniperRig.transform.localRotation = Quaternion.Euler(SniperRigTurretEulerSingleAxis());
            //rotate the sniperRig gun -> affects our camera
            sniperRigGun.transform.LookAt(stabilizerPos2);
            sniperRigGun.transform.localRotation = Quaternion.Euler(SniperRigGunEulerSingleAxis());

            //control snipercam
            camRotY = sniperSpeedY * _mouseY * (sniperCamera.fieldOfView/61f);
            camRotX = sniperSpeedX * _mouseX * (sniperCamera.fieldOfView/61f);
            sniperRig.transform.eulerAngles += new Vector3(0f, camRotX, 0f);
            sniperRigGun.transform.eulerAngles -= new Vector3(camRotY, 0f, 0f);
            //attempt at clamping
            Vector3 clampthis = sniperRigGun.transform.localEulerAngles;
            if(clampthis.x>minGunAngle && clampthis.x<300f){ clampthis.x=minGunAngle;}
            if(clampthis.x<(360f-maxGunAngle) && clampthis.x>300f){ clampthis.x=(360f-maxGunAngle);}
            sniperRigGun.transform.localEulerAngles = clampthis;
            //I dont remember what this is 
            stabilizerPos=GetStabilizerPosition();
            if(camRotY!=0f){
                stabilizerPos2=stabilizerPos;
            }
        }
    }

    private Vector3 SniperRigTurretEulerSingleAxis(){
        Vector3 _newTurretRot = sniperRig.transform.localEulerAngles;
        _newTurretRot.x = 0;
        _newTurretRot.z = 0;
        return _newTurretRot;
    }

    private Vector3 SniperRigGunEulerSingleAxis(){
        Vector3 _newGunRot = sniperRigGun.transform.localEulerAngles;
        _newGunRot.y = 0;
        _newGunRot.z = 0;
        return _newGunRot;
    }

    private void Zoom(bool isSniperMode){
        int pointer,maxlen;
        pointer = isSniperMode ? zoomPointer : zoomPointerNormal;
        maxlen = isSniperMode ? zoomStepsSniperMode.Length-1 : zoomStepsNormal.Length-1;
        //check if we used scrollwheel
        if(_mouseScroll!=0f){
            if(_mouseScroll>0f){
                pointer+=1;
            }else{
                pointer-=1;
            }
        }
        //check whether we exceeded limits and wanted to change modes
        if(pointer<0){
            pointer=0;
            if(isSniperMode){
                GetOutOfSniperMode();
            }
        }
        if(pointer>maxlen){
            pointer=maxlen;
            if(!isSniperMode){
                GetOutOfNormalMode();
            }
        }
        //apply zoom
        if(isSniperMode){
            sniperCamera.fieldOfView = zoomStepsSniperMode[pointer];
            zoomPointer = pointer;
        }else{
            currentDistance = zoomStepsNormal[pointer];
            zoomPointerNormal = pointer;
        }
    }

    private void GetOutOfSniperMode(){
        inSniperMode = false;
        currentDistance = zoomStepsNormal[zoomPointerNormal];
        sniperCamera.fieldOfView = zoomStepsSniperMode[0];
        ourCamera.enabled = true;
        sniperCamera.enabled = false;
    }

    private void GetOutOfNormalMode(){
        inSniperMode = true;
        sniperCamera.enabled = true;
        ourCamera.enabled = false;
        stabilizerPos = GetStabilizerPosition();
        stabilizerPos2 = stabilizerPos;
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
                //Debug.Log(hit2.distance.ToString());
            }
        }
        return _pos;
    }

    public Vector3 GetStabilizerPosition(){
        Vector3 _pos;
        if(!inSniperMode){
            _pos = transform.position + (transform.forward * 1000);
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit3, 100f, StabilizerLayer)){
                _pos = hit3.point;
                //Debug.DrawLine(transform.position, hit3.point, Color.blue);
            }
        }else{
            _pos = sniperRigGun.transform.position + (sniperRigGun.transform.forward * 1000);
            if (Physics.Raycast(sniperRigGun.transform.position, sniperRigGun.transform.forward, out RaycastHit hit2, 100f, StabilizerLayer)){
                _pos = hit2.point;
            }
        }
        //targetPos.transform.position = _pos;
        return _pos;
    }
}