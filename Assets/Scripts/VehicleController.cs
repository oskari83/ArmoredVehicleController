using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleController : MonoBehaviour{
    [Header("Tank power settings")]
    public float turnTorque = 4f;
    public float driveTorque = 4f;
    public float brakeStrength = 2.5f;
    public float maxRot = 0.75f;
    public float maxSpeed = 20f;

    [Header("Tank Turning Friction")]
    public float movementSidewaysFriction = 2.2f;
    public float stillSidewaysFriction = 0.8f;
    private WheelFrictionCurve[] sFrictionLeft;
    private WheelFrictionCurve[] sFrictionRight;

    [Header("Turret and Gun Settings")]
    [SerializeField] private float turretTraverseSpeed = 0.5f;
    [SerializeField] private float gunTraverseSpeed = 1.5f;
    public int maxGunAngle = 35;
    public int minGunAngle = 5;
    public Vector3 TurretTargetPosition {get; set;}

    private Vector3 targetPosition;
    private Quaternion gunLocalRotation;
    private Quaternion turretLocalRotation;

    [Header("Gameobjects")]
    public CameraController camController;
    public GameObject turretGO;
    public GameObject gunGO;
    public GameObject gunShootPos;
    public GameObject crossHair;
    public GameObject sniperTurret;
    public GameObject sniperGun;
    public Camera sniperCam;
    public GameObject visualGun;
    public GameObject visualMuzzle;
    public GameObject visualTurret;

    [HideInInspector]
    public Camera cameraInUse;
    private Camera mainCam;


    [Header("Inputs and velocity")]
    public float aVeL;
    public float VeL;
    public float height;
    public float maxHeight = 1.15f;
    private float driveInput;
    private float turnInput;
    
    private float lastAngle;
    private float curAngle;

    #region variables
    [Header("Tank Wheel and Visual/Sound effects")]
	[SerializeField] private WheelCollider[] leftWheelColliders;
    [SerializeField] private WheelCollider[] rightWheelColliders;
	[SerializeField] private Transform[] leftWheelMeshes;
    [SerializeField] private Transform[] leftDummyWheelMeshes;
    [SerializeField] private Transform[] rightWheelMeshes;
    [SerializeField] private Transform[] rightDummyWheelMeshes;
    [SerializeField] private Transform[] leftTrackBones;
    [SerializeField] private Transform[] rightTrackBones;
    [SerializeField] private Transform leftTrack;
    [SerializeField] private Transform rightTrack;
	[SerializeField] public float trackThiccness = 0.14f;
    [SerializeField] private float trackSpeed = 0.005f;
    [SerializeField] public float centerOfMassYOffset = -1.0f;
    [SerializeField] public float roadWheelSpinMultiplier = 250.0f;
    [SerializeField] public float dummyWheelSpinMultiplier = 150.0f;
    [SerializeField] public AudioSource engineAudioSource;
    [SerializeField] public float maxPitch;
    [SerializeField] public float idlePitch;

	private Vector3 colliderPos;
	private Quaternion colliderRot;
    private bool brakeInput;
    private int trackOffset;
    private Rigidbody rigidBody;
    private Material leftTrackMaterial;
    private Material rightTrackMaterial;
    private float localZVelocity;
    private float leftDirectionalMultiplier = 1.0f; 
    private float rightDirectionalMultiplier = 1.0f; 
    private float curPitch = 0.5f;
    private Vector3 finalCrosshairPos;
    #endregion

    private bool grounded = true;
    public float dampingCoefficient = 100000f;

    private void Start(){
        if(mainCam==null){
            mainCam = camController.ourCamera;
        }
        cameraInUse = mainCam;
        rigidBody = this.GetComponent<Rigidbody>();
        leftTrackMaterial = leftTrack.GetComponent<Renderer>().material;
        rightTrackMaterial = rightTrack.GetComponent<Renderer>().material;

        TurretTargetPosition = transform.position + (transform.forward * 2);
        targetPosition = TurretTargetPosition;

        sFrictionLeft = new WheelFrictionCurve[leftWheelColliders.Length];
        sFrictionRight = new WheelFrictionCurve[rightWheelColliders.Length];

        for (int i = 0; i < leftWheelColliders.Length; i++) {
            sFrictionLeft[i] = leftWheelColliders[i].sidewaysFriction;
            sFrictionLeft[i].stiffness = stillSidewaysFriction;
            leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            sFrictionRight[i] = rightWheelColliders[i].sidewaysFriction;
            sFrictionRight[i].stiffness = stillSidewaysFriction;
            rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
        }

        turnTorque = turnTorque * 1000000f;
        driveTorque = driveTorque * 100f;
        brakeStrength = brakeStrength * 1000f;

        rigidBody.maxAngularVelocity = maxRot;
    }

    private void Update() {
        driveInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1, 1);
        turnInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1, 1);
        brakeInput = Input.GetKey("space");

        targetPosition = TurretTargetPosition;
        CrosshairMovement();
        UpdateTurret();
    }

    private void FixedUpdate() {
        rigidBody.centerOfMass = new Vector3(0, centerOfMassYOffset, 0);
        localZVelocity = transform.InverseTransformDirection(rigidBody.velocity).z;

        //set boolean to indicate whether we are on the ground
        GetHeight();
        grounded = (height<= maxHeight) ? true : false;

        //only turn if we are on the groun
        if(grounded){
            if(driveInput<0){
                rigidBody.AddTorque(transform.up * -turnInput * turnTorque * Time.fixedDeltaTime);
            }else{
                rigidBody.AddTorque(transform.up * turnInput * turnTorque * Time.fixedDeltaTime);
            }

            if(turnInput==0){
                //for responsive feel so that tank doest continue turning after input
                rigidBody.AddTorque(transform.up * turnTorque * Time.fixedDeltaTime * -rigidBody.angularVelocity.y);
            }
        }
        

        //show our velocity and angular velocity in inspector
        aVeL = rigidBody.angularVelocity.magnitude;
        VeL = rigidBody.velocity.magnitude * 3.6f;

        
        if(driveInput!=0f){
            SetLeftTrackTorque(driveInput * driveTorque);
            SetRightTrackTorque(driveInput * driveTorque);
        }else if(turnInput!=0f){
            SetLeftTrackTorque(0.01f * driveTorque);
            SetRightTrackTorque(0.01f * driveTorque);
        }else{
            SetLeftTrackTorque(0f);
            SetRightTrackTorque(0f);
        }
        
        //limit our max velocity and angularvelocity
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);
        rigidBody.angularVelocity = Vector3.ClampMagnitude(rigidBody.angularVelocity, maxRot);
        
        
        if((driveInput==0 && turnInput==0) || (driveInput<0f && rigidBody.velocity.magnitude>5f)){
            rigidBody.drag = 2;
            SetBrakes(brakeStrength);
        }else if (driveInput==0 && turnInput!=0 && rigidBody.velocity.magnitude>1f){
            rigidBody.drag = 2f;
            SetBrakes(brakeStrength);
        }else{
            rigidBody.drag = 0.1f;
            SetBrakes(0);
        }
        

        //get out current climb angle
        GetClimbAngle();
        if(curAngle>20f && height<= maxHeight && lastAngle<curAngle){
            rigidBody.drag=curAngle*0.03f;
        }
        lastAngle = curAngle;
    }

    private void GetHeight(){
        //how high off the ground are we
        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -transform.up);
        //Debug.DrawRay(downRay.origin,downRay.direction * 100, Color.red);
        if (Physics.Raycast(downRay, out hit)){
            height = hit.distance;
        }
    }

    private void GetClimbAngle(){
        curAngle = Vector3.Angle(Vector3.up, transform.up);
    }

    public void EnableVisuals(){
        visualGun.GetComponent<MeshRenderer>().enabled = true;
        visualMuzzle.GetComponent<MeshRenderer>().enabled = true;
        visualTurret.GetComponent<MeshRenderer>().enabled = true;
    }

    public void DisableVisuals(){
        visualGun.GetComponent<MeshRenderer>().enabled = false;
        visualMuzzle.GetComponent<MeshRenderer>().enabled = false;
        visualTurret.GetComponent<MeshRenderer>().enabled = false;
    }

    #region Torque, brake
    private void SetLeftTrackTorque(float speed) {
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].motorTorque = speed;
        }
    }

    private void SetRightTrackTorque(float speed) {
        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].motorTorque = speed;
        }
    }

    public void SetBrakes(float strength) {
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].brakeTorque = strength;
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].brakeTorque = strength;
        }
    }
    #endregion

    #region crosshair
    private void CrosshairMovement(){
        if(camController.inSniperMode){
            cameraInUse = sniperCam;
        }else{
            cameraInUse = mainCam;
        }
        //get gun shoot pos object
        GameObject _gun = gunGO;
        float _maxCrosshairDistance = 10000f;
        Vector3 _forwardGun = _gun.transform.position + (_gun.transform.forward * _maxCrosshairDistance);
        // check for obstacles in front of the cannon
        Ray crossHairRay;
        crossHairRay = new Ray(_gun.transform.position,  _gun.transform.forward);
        if (Physics.Raycast(crossHairRay, out RaycastHit hit, _maxCrosshairDistance)){
            _forwardGun = hit.point;
            //Debug.DrawLine(crossHairRay.origin, hit.point, Color.green);
        }
        // placing the target's sprite in front of the cannon
        Quaternion _lookToHit = Quaternion.LookRotation(_forwardGun - cameraInUse.transform.position);
        Vector3 _crosshairPos = cameraInUse.transform.position + (_lookToHit * Vector3.forward);
        // disable the sight if the is not in front of the camera
        float _angleBetweenGunAndCamera = Mathf.Abs(Vector3.Angle(_gun.transform.forward, cameraInUse.transform.forward));
        //crossHair.GetComponent<Image>().enabled = _angleBetweenGunAndCamera > cameraInUse.fieldOfView ? false : true;
        if(_angleBetweenGunAndCamera > cameraInUse.fieldOfView){
            crossHair.SetActive(false);
        }else{
            crossHair.SetActive(true);
        }
        // convert the world position of the crosshairs to the position of the screen (smoothed)
        finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, cameraInUse.WorldToScreenPoint(_crosshairPos), Time.deltaTime * 10f);
        crossHair.transform.position = finalCrosshairPos;
    }
    #endregion

    #region turretTurn
    private void UpdateTurret(){
        // look to target
        Quaternion _lookAtTurret = Quaternion.LookRotation(targetPosition - turretGO.transform.position, gameObject.transform.up);
        Quaternion _lookAtGun = Quaternion.LookRotation(targetPosition - gunGO.transform.position, gunGO.transform.up);
        Quaternion _turretRelativeRotTarget = Quaternion.Euler(gameObject.transform.eulerAngles - _lookAtTurret.eulerAngles);
        Quaternion _gunRelativeRotTarget = Quaternion.Euler(turretGO.transform.eulerAngles - _lookAtGun.eulerAngles);
        float _angleBetweenTurretAndTarget = Vector3.Angle(turretLocalRotation * Vector3.forward, _turretRelativeRotTarget * Vector3.forward);
        float _angleBetweenGunAndTarget = Vector3.Angle(gunLocalRotation * Vector3.forward, _gunRelativeRotTarget * Vector3.forward);
        float _turretVelocity = 1 / _angleBetweenTurretAndTarget;
        float _gunVelocity = 1 / _angleBetweenGunAndTarget;
        float _horizontalSpeed = turretTraverseSpeed;
        _horizontalSpeed *= _turretVelocity;
        _horizontalSpeed *= Time.deltaTime;
        float _verticalSpeed = gunTraverseSpeed;
        _verticalSpeed *= _gunVelocity;
        _verticalSpeed *= Time.deltaTime;
        Quaternion _turretFinalRotation = Quaternion.Euler(gameObject.transform.eulerAngles - _lookAtTurret.eulerAngles);
        Quaternion _gunFinalRotation = Quaternion.Euler(turretGO.transform.eulerAngles - _lookAtGun.eulerAngles);

        if (Input.GetMouseButton(1)) { Debug.Log("holding down"); } else{
            turretLocalRotation = Quaternion.Lerp(turretLocalRotation, _turretFinalRotation, _horizontalSpeed);
            gunLocalRotation = Quaternion.Lerp(gunLocalRotation, _gunFinalRotation, _verticalSpeed);
            Quaternion _turretRot = Quaternion.Euler(gameObject.transform.eulerAngles - turretLocalRotation.eulerAngles);
            Quaternion _gunRot = Quaternion.Euler(turretGO.transform.eulerAngles - gunLocalRotation.eulerAngles);
            gunGO.transform.rotation = _gunRot;
            turretGO.transform.rotation = _turretRot;
            Vector3 _newGunRotation = gunGO.transform.localEulerAngles;
            Vector3 _newTurretRotation = turretGO.transform.localEulerAngles;

            {
                float _max = 360 - maxGunAngle;
                float _min = minGunAngle;
                float _currentAngle = gunGO.transform.localEulerAngles.x;
                if (_currentAngle > 180){
                        if (_currentAngle < _max) _newGunRotation.x = _max;
                }
                else{
                        if (_currentAngle > _min) _newGunRotation.x = _min;
                }
            }

            _newTurretRotation.x = 0;
            _newTurretRotation.z = 0;
            _newGunRotation.y = 0;
            _newGunRotation.z = 0;
            //apply local rotation
            turretGO.transform.localRotation = Quaternion.Euler(_newTurretRotation);
            gunGO.transform.localRotation = Quaternion.Euler(_newGunRotation);
        }

    }
    #endregion

    void LateUpdate(){
        //We set the wheel mesh and bones (track deform) positions to the positions of wheel colliders
        for (int i = 0; i < leftWheelColliders.Length; i++) {
            leftWheelColliders[i].GetWorldPose(out colliderPos, out colliderRot);
            leftWheelMeshes[i].position = colliderPos + new Vector3 (0, trackThiccness, 0);
            leftTrackBones[i].position = leftWheelMeshes[i].position + transform.up * -1.0f * leftWheelColliders[i].radius;
        }

        for (int i = 0; i < rightWheelColliders.Length; i++) {
            rightWheelColliders[i].GetWorldPose(out colliderPos, out colliderRot);
            rightWheelMeshes[i].position = colliderPos + new Vector3 (0, trackThiccness, 0);
            rightTrackBones[i].position = rightWheelMeshes[i].position + transform.up * -1.0f * leftWheelColliders[i].radius;
		}

        //If we are still or almost still reduce sideways friction
        if (rigidBody.velocity.magnitude <= 1f || curAngle > 22f) {
            for (int i = 0; i < leftWheelColliders.Length; i++) {
                sFrictionLeft[i].stiffness = stillSidewaysFriction;
                leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
            }

            for (int i = 0; i < rightWheelColliders.Length; i++) {
                sFrictionRight[i].stiffness = stillSidewaysFriction;
                rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
            }
        }
        else {
            for (int i = 0; i < leftWheelColliders.Length; i++) {
                sFrictionLeft[i].stiffness = movementSidewaysFriction;
                leftWheelColliders[i].sidewaysFriction = sFrictionLeft[i];
            }

            for (int i = 0; i < rightWheelColliders.Length; i++) {
                sFrictionRight[i].stiffness = movementSidewaysFriction;
                rightWheelColliders[i].sidewaysFriction = sFrictionRight[i];
            }
        }

        //We rotate the wheels to simulate movement - I don't use the out rot from wheel collider because I want everything to spin at the same rate
        foreach (Transform wheelMesh in leftWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * localZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in leftDummyWheelMeshes) {
            wheelMesh.Rotate(leftDirectionalMultiplier * localZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        foreach (Transform wheelMesh in rightWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * localZVelocity * trackSpeed * roadWheelSpinMultiplier, 0, 0, Space.Self);
        }
        foreach (Transform wheelMesh in rightDummyWheelMeshes) {
            wheelMesh.Rotate(rightDirectionalMultiplier * localZVelocity * trackSpeed * dummyWheelSpinMultiplier, 0, 0, Space.Self);
        }

        //We scroll the track texture to simulate movement
        leftTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, leftTrackMaterial.mainTextureOffset.y + (leftDirectionalMultiplier * -1.0f * rigidBody.velocity.magnitude * trackSpeed * Mathf.Sign(localZVelocity))));
        rightTrackMaterial.SetTextureOffset("_MainTex", new Vector2(0, rightTrackMaterial.mainTextureOffset.y + (rightDirectionalMultiplier * -1.0f * rigidBody.velocity.magnitude * trackSpeed * Mathf.Sign(localZVelocity))));
   
        //set audio
        curPitch = Mathf.Clamp(idlePitch + rigidBody.velocity.magnitude / 40.0f, idlePitch, maxPitch);
        engineAudioSource.pitch = curPitch;
    }
}
