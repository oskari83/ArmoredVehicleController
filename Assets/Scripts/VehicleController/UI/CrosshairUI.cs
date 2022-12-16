using UnityEngine;

public class CrosshairUI : MonoBehaviour{

	public CameraController cameraController;
	private Camera cameraInUse;

	[Header("Gameobjects")]
	public GameObject crossHair;

	private Camera mainCamera;
	private Camera sniperCamera;
	private VehicleControllerManager vehicleManager;
	private Vector3 finalCrosshairPos;

	private void Awake(){
		if(mainCamera==null){
            mainCamera = cameraController.ourCamera;
        }
        cameraInUse = mainCamera;
		vehicleManager = GetComponent<VehicleControllerManager>();
		sniperCamera = vehicleManager.sniperModeCamera;
	}

	private void Update(){
		MoveCrosshair();
	}

	private void MoveCrosshair(){
        if(cameraController.inSniperMode){
            cameraInUse = sniperCamera;
        }else{
            cameraInUse = mainCamera;
        }
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
}