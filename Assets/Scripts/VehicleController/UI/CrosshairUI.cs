using UnityEngine;

public class CrosshairUI : MonoBehaviour{

	[Header("Crosshair Gameobject")]
	public GameObject crossHair;

	[Header("Crosshair Dispersion Gameobjects")]
    public RectTransform crosshairCircleRectTransformImproved;
	public GameObject crosshairSpread;
    public GameObject crosshairSpreadx2;
    public GameObject crosshairSpread_v;
    public GameObject crosshairSpreadx2_v;

	private VehicleControllerManager vehicleManager;
	private ShootingController shootingController;
	private Camera cameraInUse;
	private Vector3 finalCrosshairPos;

	private void Awake(){
		vehicleManager = GetComponent<VehicleControllerManager>();
		shootingController = GetComponent<ShootingController>();
	}

	private void Update(){
		MoveCrosshair();
		MoveDispersionCrosshairs();
	}

	private void MoveCrosshair(){
		cameraInUse = vehicleManager.CameraInUse;
        // Get gun shoot pos object
		GameObject _gun = vehicleManager.tankGunGameObject;
        float _maxCrosshairDistance = 10000f;
        Vector3 _forwardGun = _gun.transform.position + (_gun.transform.forward * _maxCrosshairDistance);
        // Check for obstacles in front of the cannon
        Ray crossHairRay;
        crossHairRay = new Ray(_gun.transform.position,  _gun.transform.forward);
        if (Physics.Raycast(crossHairRay, out RaycastHit hit, _maxCrosshairDistance)){
            _forwardGun = hit.point;
            // Debug.DrawLine(crossHairRay.origin, hit.point, Color.green);
        }
        // Placing the target's sprite in front of the cannon
        Quaternion _lookToHit = Quaternion.LookRotation(_forwardGun - cameraInUse.transform.position);
        Vector3 _crosshairPos = cameraInUse.transform.position + (_lookToHit * Vector3.forward);
        // Disable the sight if the is not in front of the camera
        float _angleBetweenGunAndCamera = Mathf.Abs(Vector3.Angle(_gun.transform.forward, cameraInUse.transform.forward));
        // CrossHair.GetComponent<Image>().enabled = _angleBetweenGunAndCamera > cameraInUse.fieldOfView ? false : true;
        if(_angleBetweenGunAndCamera > cameraInUse.fieldOfView){
            crossHair.SetActive(false);
        }else{
            crossHair.SetActive(true);
        }
        // Convert the world position of the crosshairs to the position of the screen (smoothed)
        finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, cameraInUse.WorldToScreenPoint(_crosshairPos), Time.deltaTime * 20f);
        // FinalCrosshairPos = cameraInUse.WorldToScreenPoint(_crosshairPos);
        crossHair.transform.position = finalCrosshairPos;
    }

	private void MoveDispersionCrosshairs(){
		crosshairSpread.transform.position = shootingController.CrosshairSpreadPositionLeft;
        crosshairSpreadx2.transform.position = shootingController.CrosshairSpreadPositionRight;
        crosshairSpread_v.transform.position = shootingController.CrosshairSpreadPositionUp;
        crosshairSpreadx2_v.transform.position = shootingController.CrosshairSpreadPositionDown;
		crosshairCircleRectTransformImproved.sizeDelta = shootingController.CrosshairSpreadCirclePosition;
	}
}