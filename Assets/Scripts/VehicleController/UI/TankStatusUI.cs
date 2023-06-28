using UnityEngine;
using UnityEngine.UI;

public class TankStatusUI : MonoBehaviour{

    public GameObject tankUIObject;
    public GameObject turretUIObject;
    private GameObject tankTurretGO;
    private CameraController cameraControlScript;
    private void Start(){
        cameraControlScript = gameObject.GetComponent<VehicleControllerManager>().cameraController;
        tankTurretGO = gameObject.GetComponent<VehicleControllerManager>().tankTurretGameObject;
    }


    private void Update(){
        tankUIObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, cameraControlScript.turretRigRotation));
        turretUIObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -1f * tankTurretGO.transform.localEulerAngles.y));
    }
}
