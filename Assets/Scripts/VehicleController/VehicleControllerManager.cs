using UnityEngine;

public class VehicleControllerManager : MonoBehaviour{

	public GameObject tankTurretGameObject;
	public GameObject tankGunGameObject;
	public GameObject sniperModeRigTurretGameObject;
    public GameObject sniperModeRigGunGameObject;
	public GameObject tankGunGameObjectVisual;
    public GameObject tankMuzzleGameObjectVisual;
    public GameObject tankTurretGameObjectVisual;
	public Camera sniperModeCamera;

	public TurretMovement TurretMovementScript { get; private set; }
	public Transform TankTransform { get; private set; }

	private void Awake(){
		TurretMovementScript = GetComponent<TurretMovement>();
		TankTransform = GetComponent<Transform>();
	}

	public void ToggleGunAndTurretVisuals(bool state){
        tankGunGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
        tankMuzzleGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
        tankTurretGameObjectVisual.GetComponent<MeshRenderer>().enabled = state;
    }
}