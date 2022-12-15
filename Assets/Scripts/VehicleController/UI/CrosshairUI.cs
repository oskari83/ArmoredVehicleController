using UnityEngine;

public class CrosshairUI : MonoBehaviour{

	public CameraController cameraController;
	private Camera cameraInUse;

	[Header("Gameobjects")]
	public Camera sniperCam;
	public GameObject crossHair;

	private Camera mainCam;
	private TurretMovement turretMovementScript;
	private Vector3 finalCrosshairPos;

	private void Awake(){
		if(mainCam==null){
            mainCam = cameraController.ourCamera;
        }
        cameraInUse = mainCam;
		turretMovementScript = GetComponent<TurretMovement>();
	}

	private void Update(){
		MoveCrosshair();
	}

	private void MoveCrosshair(){
        if(cameraController.inSniperMode){
            cameraInUse = sniperCam;
        }else{
            cameraInUse = mainCam;
        }
        //get gun shoot pos object
        GameObject _gun = turretMovementScript.gunGO;
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
        finalCrosshairPos = Vector3.Lerp(finalCrosshairPos, cameraInUse.WorldToScreenPoint(_crosshairPos), Time.deltaTime * 20f);
        //finalCrosshairPos = cameraInUse.WorldToScreenPoint(_crosshairPos);
        crossHair.transform.position = finalCrosshairPos;
    }
}