using UnityEngine;

public class TurretMovement : MonoBehaviour{

	[Header("Turret and Gun Settings")]
    public float turretTraverseSpeed = 45f;
    public float gunTraverseSpeed = 45f;
    public int maxGunAngle_elevation = 35;
    public int minGunAngle_depression = 8;

	public Vector3 TurretTargetPosition { get; set; }

	private VehicleControllerManager vehicleManager;
	private GameObject turretGameObject;
    private GameObject gunGameObject;
	private Camera sniperModeCamera;

    private Vector3 targetPosition;
    private Quaternion gunLocalRotation;
    private Quaternion turretLocalRotation;

	private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		turretGameObject = vehicleManager.tankTurretGameObject;
		gunGameObject = vehicleManager.tankGunGameObject;
		sniperModeCamera = vehicleManager.sniperModeCamera;
		TurretTargetPosition = transform.position + (transform.forward * 2);
        targetPosition = TurretTargetPosition;
	}

	private void Update() {
        targetPosition = TurretTargetPosition;

        MoveTurret();
    }

	private void MoveTurret(){
        // look to target
        Quaternion _lookAtTurret = Quaternion.LookRotation(targetPosition - turretGameObject.transform.position, gameObject.transform.up);
        Quaternion _lookAtGun = Quaternion.LookRotation(targetPosition - gunGameObject.transform.position, gunGameObject.transform.up);
        Quaternion _turretRelativeRotTarget = Quaternion.Euler(gameObject.transform.eulerAngles - _lookAtTurret.eulerAngles);
        Quaternion _gunRelativeRotTarget = Quaternion.Euler(turretGameObject.transform.eulerAngles - _lookAtGun.eulerAngles);
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
        Quaternion _gunFinalRotation = Quaternion.Euler(turretGameObject.transform.eulerAngles - _lookAtGun.eulerAngles);

        if(Input.GetMouseButton(1)){ 
            Debug.Log("holding down"); 
        }else{
            turretLocalRotation = Quaternion.Lerp(turretLocalRotation, _turretFinalRotation, _horizontalSpeed);
            gunLocalRotation = Quaternion.Lerp(gunLocalRotation, _gunFinalRotation, _verticalSpeed);
            Quaternion _turretRot = Quaternion.Euler(gameObject.transform.eulerAngles - turretLocalRotation.eulerAngles);
            Quaternion _gunRot = Quaternion.Euler(turretGameObject.transform.eulerAngles - gunLocalRotation.eulerAngles);
            gunGameObject.transform.rotation = _gunRot;
            turretGameObject.transform.rotation = _turretRot;
            Vector3 _newGunRotation = gunGameObject.transform.localEulerAngles;
            Vector3 _newTurretRotation = turretGameObject.transform.localEulerAngles;

            {
                float _max = 360 - maxGunAngle_elevation;
                float _min = minGunAngle_depression;
                float _currentAngle = gunGameObject.transform.localEulerAngles.x;
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
            turretGameObject.transform.localRotation = Quaternion.Euler(_newTurretRotation);
            gunGameObject.transform.localRotation = Quaternion.Euler(_newGunRotation);
        }

    }
}