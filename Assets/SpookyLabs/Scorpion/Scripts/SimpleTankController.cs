using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTankController : MonoBehaviour{
    [Header("Tank power settings")]
    public float turnTorque = 4f;
    public float driveTorque = 4f;
    public float brakeStrength = 2.5f;
    public float maxRot = 5f;
    public float maxSpeed = 40f;
    public float numMagic = 1;

    [Header("Turret and Gun Settings")]
    [SerializeField] private float turretTraverseSpeed = 0.5f;
    [SerializeField] private float gunTraverseSpeed = 1.5f;
    [SerializeField] private int maxGunAngle = 35;
    [SerializeField] private int minGunAngle = 5;
    private Vector3 targetPosition;
    private Quaternion gunLocalRotation;
    private Quaternion turretLocalRotation;
    bool turretEnabled;
    public Vector3 TurretTargetPosition {get; set;}

    [Header("Gameobjects")]
    public GameObject turretGO;
    public GameObject gunGO;
    public GameObject crossHair;
    public CameraController camController;
    public Camera sniperCam;
    public Camera mainCam;

    [Header("Inputs and velocity")]
    public float aVeL;
    public float VeL;
    public float height;
    public float maxHeight = 1.15f;
    private float driveInput;
    private float turnInput;

    [Header("Tank Turning Friction")]
    public float movementSidewaysFriction = 2.2f;
    public float stillSidewaysFriction = 0.8f;
    private WheelFrictionCurve[] sFrictionLeft;
    private WheelFrictionCurve[] sFrictionRight;

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
    [SerializeField] public ParticleSystem[] groundFxs; 
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

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        rigidBody = this.GetComponent<Rigidbody>();
        leftTrackMaterial = leftTrack.GetComponent<Renderer>().material;
        rightTrackMaterial = rightTrack.GetComponent<Renderer>().material;
        SetGroundFxsState(false);

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
    }

    #region Torque, brake and groundFX
    private void SetGroundFxsState(bool state) {
        foreach (ParticleSystem fx in groundFxs) {
            ParticleSystem.EmissionModule eMod = fx.emission;
            eMod.enabled = state;
        }
    }

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

    private void Update() {
        driveInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1, 1);
        turnInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1, 1);
        brakeInput = Input.GetKey("space");

        targetPosition = TurretTargetPosition;
        CrosshairMovement();
    }

    #region crosshair
    private void CrosshairMovement(){
        var _mainCamera = Camera.main;
        if(camController.inSniperMode){
            _mainCamera = sniperCam;
        }else{
            _mainCamera = Camera.main;
        }

        var _gun = gunGO;
        var _maxCrosshairDistance = 5000f;
        var _forwardGun = _gun.transform.position + (_gun.transform.forward * _maxCrosshairDistance);
        // check for obstacles in front of the cannon
        Ray crossHairRay;
        crossHairRay = new Ray(_gun.transform.position,  _gun.transform.forward);
        if (Physics.Raycast(crossHairRay, out RaycastHit hit, _maxCrosshairDistance)){
            _forwardGun = hit.point;
            Debug.DrawLine(crossHairRay.origin, hit.point, Color.green);
        }

        // placing the target's sprite in front of the cannon
        var _lookToHit = Quaternion.LookRotation(_forwardGun - _mainCamera.transform.position);
        var _crosshairPos = _mainCamera.transform.position + (_lookToHit * Vector3.forward);

        // disable the sight if the is not in front of the camera
        var _angleBetweenGunAndCamera = Mathf.Abs(Vector3.Angle(_gun.transform.forward, _mainCamera.transform.forward));
        crossHair.GetComponent<Image>().enabled = _angleBetweenGunAndCamera > _mainCamera.fieldOfView ? false : true;

        // convert the world position of the crosshairs to the position of the screen (smoothed)
        finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, _mainCamera.WorldToScreenPoint(_crosshairPos), Time.deltaTime * 10f);
        crossHair.transform.position = finalCrosshairPos;
    }
    #endregion

    void FixedUpdate() {
        this.transform.GetComponent<Rigidbody>().centerOfMass = new Vector3(0, centerOfMassYOffset, 0);
        localZVelocity = transform.InverseTransformDirection(rigidBody.velocity).z;

        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -Vector3.up);

        if (Physics.Raycast(downRay, out hit)){
            height = hit.distance;
        }

        rigidBody.maxAngularVelocity = maxRot;
        if(height<= maxHeight){
            if(driveInput<0){
                rigidBody.AddTorque(transform.up * -turnInput * turnTorque * Time.deltaTime);
            }else{
                rigidBody.AddTorque(transform.up * turnInput * turnTorque * Time.deltaTime);
            }
            if(turnInput==0){
                rigidBody.angularDrag=12;
                rigidBody.angularVelocity = Vector3.zero;
            }else{
                rigidBody.angularDrag=1;
            }
        }else{
            rigidBody.angularDrag=0.1f;
        }

        aVeL = rigidBody.angularVelocity.magnitude;
        VeL = rigidBody.velocity.magnitude;

        if(driveInput!=0f){
            SetLeftTrackTorque(driveInput * driveTorque);
            SetRightTrackTorque(driveInput * driveTorque);
        }else if(turnInput!=0){
            if(turnInput>0){
                SetLeftTrackTorque(numMagic * driveTorque);
                SetRightTrackTorque(-numMagic * driveTorque);
            }else{
                SetLeftTrackTorque(-numMagic * driveTorque);
                SetRightTrackTorque(numMagic * driveTorque);
            }
        }else{
            SetLeftTrackTorque(0f);
            SetRightTrackTorque(0f);
        }

        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);

        if((driveInput==0 && turnInput==0) || (driveInput<0f && rigidBody.velocity.magnitude>1f)){
            rigidBody.drag = 2;
            SetBrakes(brakeStrength);
        }else if (driveInput==0 && turnInput!=0 && rigidBody.velocity.magnitude>1f){
            rigidBody.drag = 0.5f;
            SetBrakes(brakeStrength);
        }else{
            rigidBody.drag = 0.1f;
            SetBrakes(0);
        }

        UpdateTurret();
    }

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
        var _horizontalSpeed = turretTraverseSpeed;
        _horizontalSpeed *= _turretVelocity;
        var _verticalSpeed = gunTraverseSpeed;
        _verticalSpeed *= _gunVelocity;
        Quaternion _turretFinalRotation = Quaternion.Euler(gameObject.transform.eulerAngles - _lookAtTurret.eulerAngles);
        Quaternion _gunFinalRotation = Quaternion.Euler(turretGO.transform.eulerAngles - _lookAtGun.eulerAngles);

        if (Input.GetMouseButton(1)) { Debug.Log("holding down"); } else{
            turretLocalRotation = Quaternion.Lerp(turretLocalRotation, _turretFinalRotation, _horizontalSpeed);
            gunLocalRotation = Quaternion.Lerp(gunLocalRotation, _gunFinalRotation, _verticalSpeed);
            // apply rotation on world space
            Quaternion _turretRot = Quaternion.Euler(gameObject.transform.eulerAngles - turretLocalRotation.eulerAngles);
            Quaternion _gunRot = Quaternion.Euler(turretGO.transform.eulerAngles - gunLocalRotation.eulerAngles);
            // --- apply rotation
            gunGO.transform.rotation = _gunRot;
            turretGO.transform.rotation = _turretRot;
            // --- clamp vertical rotation
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
            // --- apply local rotation
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

        //We need to give some more push to the track speed if we're rotating in place
        if (rigidBody.velocity.magnitude <= 1f) {
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
   
        //We toggle the ground dust particle effect depending on our speed
        if (rigidBody.velocity.magnitude > 4) {
            SetGroundFxsState(true);
        }
        else SetGroundFxsState(false);

        curPitch = Mathf.Clamp(idlePitch + rigidBody.velocity.magnitude / 40.0f, idlePitch, maxPitch);
        engineAudioSource.pitch = curPitch;
    }
}
