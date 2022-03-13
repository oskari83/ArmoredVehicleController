using System;
using UnityEngine;
using UnityEngine.UI;

public class MMV_CameraController : MonoBehaviour{
    [Serializable] public class Position{
        [SerializeField] private float height;
        [SerializeField] private float minAngle;
        [SerializeField] private float maxAngle;
        [SerializeField] private float rotSpeed;

        public float MinAngle { get => minAngle; set => minAngle = value; }
        public float MaxAngle { get => maxAngle; set => maxAngle = value; }
        public float RotSpeed { get => rotSpeed; set => rotSpeed = value; }
        public float Height { get => height; set => height = value; }
    }

    [Serializable] public class ZoomCamera{
        [SerializeField] private float maxCamDistance;
        [SerializeField] private float minCamDistance;
        [SerializeField] private float camZoomSpeed;

        public float MaxCamDistance { get => maxCamDistance; set => maxCamDistance = value; }
        public float MinCamDistance { get => minCamDistance; set => minCamDistance = value; }
        public float CamZoomSpeed { get => camZoomSpeed; set => camZoomSpeed = value; }
    }

    [Serializable] public class Crosshair{
        [SerializeField] private int maxDistance;
        [SerializeField] private LayerMask obstaclesLayer;

        public int MaxDistance { get => maxDistance; set => maxDistance = value; }
        public LayerMask ObstaclesLayer { get => obstaclesLayer; set => obstaclesLayer = value; }
    }

    [SerializeField] private VehicleController vehicle;
    [SerializeField] private Position movement;
    [SerializeField] private ZoomCamera zoom;
    [SerializeField] private Crosshair gunCrosshair;

    //----------------------------------------------------------------

    public VehicleController Vehicle { get => vehicle; set => vehicle = value; }
    public ZoomCamera Zoom { get => zoom; set => zoom = value; }
    public Crosshair GunCrosshair { get => gunCrosshair; set => gunCrosshair = value; }
    public Position CameraPosition { get => movement; set => movement = value; }

    //--------------------------------------------------------------

    // distance from the front of the camera radius collision
    private const float CAMERA_COLLISION_FORWARD_HIT_OFFSET = 0.1f;

    private float currentRotX;
    private float currentRotY;
    private float currentDistance;

    [SerializeField] public float sniperZoomSpeed = 1f;
    [SerializeField] public Camera sniperCamera;
    [SerializeField] public Camera ourCamera;
    public bool inSniperMode = false;

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!vehicle){
            Debug.LogWarning("Vehicle not assigned");
            return;
        }

        ourCamera = gameObject.GetComponent<Camera>();

        // initial camera transform
        transform.position = (vehicle.transform.position + -vehicle.transform.forward) + (Vector3.up * 2);
        //transform.eulerAngles = Vector3.up * vehicle.transform.eulerAngles.y;
        transform.LookAt(vehicle.transform.position + (Vector3.up * movement.Height));

        // set camera distance of vehicle on middle of min_distance and max_distance
        currentDistance = zoom.MinCamDistance + (zoom.MaxCamDistance - zoom.MinCamDistance) / 2;
    }

    Vector3 _vel;

    void Update(){
        if (!vehicle){
            return;
        }

        ControlCamera();
        if(!inSniperMode)
            ControlTurret();

        Vector3 _cameraPos = (vehicle.transform.position - (transform.forward * currentDistance)) + (Vector3.up * movement.Height);

        // camera collision
        if (Physics.Linecast(vehicle.transform.position, _cameraPos, out RaycastHit hit, 1 << gunCrosshair.ObstaclesLayer)){
            _cameraPos = (hit.point + transform.forward * CAMERA_COLLISION_FORWARD_HIT_OFFSET);
        }

        transform.position = _cameraPos;
    }

    private void ControlCamera(){
        float _mouseY = Input.GetAxis("Mouse Y");
        float _mouseX = Input.GetAxis("Mouse X");

        if(inSniperMode){
            sniperCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sniperZoomSpeed;
        }else{
            currentDistance += Input.GetAxis("Mouse ScrollWheel") * zoom.CamZoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, zoom.MinCamDistance, zoom.MaxCamDistance);
        }

        //Debug.Log(currentDistance.ToString());
        if(!inSniperMode){
            if(currentDistance < zoom.MinCamDistance + 1.5f){
                inSniperMode = true;
                sniperCamera.enabled = true;
                ourCamera.enabled = false;
            }

            currentRotX += _mouseX * movement.RotSpeed;
            currentRotY -= _mouseY * movement.RotSpeed;

                // clamp vertical camera rotation
            currentRotY = Mathf.Clamp(currentRotY, -movement.MinAngle, movement.MaxAngle);
            transform.eulerAngles = new Vector3(currentRotY, currentRotX);

            if (currentRotX > 360) currentRotX -= 360;
            if (currentRotX < 0) currentRotX += 360;

        }else{
            if(sniperCamera.fieldOfView >= 61){
                inSniperMode = false;
                currentDistance = zoom.MinCamDistance + 1.5f;
                sniperCamera.fieldOfView = 60.5f;
                ourCamera.enabled = true;
                sniperCamera.enabled = false;
            }
        }

        sniperCamera.fieldOfView = Mathf.Clamp(sniperCamera.fieldOfView, 10, 61);
    }

    private void ControlTurret(){
        vehicle.TurretTargetPosition = GetTargetPosition();
    }

    public Vector3 GetTargetPosition(){
        // default target position is forward of camera
        Vector3 _pos = Camera.main.transform.position + (Camera.main.transform.forward * 1000);

        // check if exist some obstacle on front
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, gunCrosshair.MaxDistance, 1 << gunCrosshair.ObstaclesLayer))
        {
            _pos = hit.point;
        }

        return _pos;
    }
}
