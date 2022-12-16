using UnityEngine;

public class VehicleControllerManager : MonoBehaviour{

	public TurretMovement TurretMovementScript { get; private set; }
	public Transform TankTransform { get; private set; }
	public Camera CameraInUse { get; private set; }
	public float LocalZVelocity { get; private set; }
	public float VelocityInKMH { get; private set; }
	public float VelocityInMPS { get; private set; }

	public CameraController cameraController;

	public GameObject tankTurretGameObject;
	public GameObject tankGunGameObject;

	public GameObject gunShootingPositionGameObject;

	public GameObject sniperModeRigTurretGameObject;
    public GameObject sniperModeRigGunGameObject;

	public GameObject tankGunGameObjectVisual;
    public GameObject tankMuzzleGameObjectVisual;
    public GameObject tankTurretGameObjectVisual;

	public Camera sniperModeCamera;

	private Rigidbody tankRigidbody;
	private Camera mainCamera;

	private void Awake(){
		TurretMovementScript = GetComponent<TurretMovement>();
		TankTransform = GetComponent<Transform>();
		tankRigidbody = GetComponent<Rigidbody>();

		if(mainCamera==null){
            mainCamera = cameraController.ourCamera;
        }
        CameraInUse = mainCamera;
	}

	private void Update(){
		GetCurrentlyUsedCamera();
	}

	private void FixedUpdate(){
		// Get Z-velocity for track visuals
		LocalZVelocity = transform.InverseTransformDirection(tankRigidbody.velocity).z;
		VelocityInKMH = tankRigidbody.velocity.magnitude * 3.6f;
		VelocityInMPS = tankRigidbody.velocity.magnitude;
	}

	private void GetCurrentlyUsedCamera(){
		if(cameraController.inSniperMode){
			CameraInUse = sniperModeCamera;
		}else{
			CameraInUse = mainCamera;
		}
	}

	public void ToggleGunAndTurretVisuals(bool state){
        tankGunGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
        tankMuzzleGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
        tankTurretGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
    }
}