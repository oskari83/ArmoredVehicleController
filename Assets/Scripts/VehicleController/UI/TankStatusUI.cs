using UnityEngine;
using UnityEngine.UI;

public class TankStatusUI : MonoBehaviour{

    public GameObject tankUIObject;
    public GameObject turretUIObject;
    private GameObject tankTurretGO;
    private CameraController cameraControlScript;
    private TankMovement movementScript;
    private Armor healthScript;

    public Image healthBarImage;
    public Text healthBarText;
    public Text tankSpeedText;

    private void Start(){
        cameraControlScript = gameObject.GetComponent<VehicleControllerManager>().cameraController;
        tankTurretGO = gameObject.GetComponent<VehicleControllerManager>().tankTurretGameObject;
        healthScript = gameObject.GetComponent<Armor>();
        movementScript = gameObject.GetComponent<TankMovement>();
    }


    private void Update(){
        tankUIObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, cameraControlScript.turretRigRotation));
        turretUIObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -1f * tankTurretGO.transform.localEulerAngles.y));

        healthBarImage.fillAmount = healthScript.tankHealth / healthScript.initialTankHealth;
        healthBarText.text = healthScript.tankHealth.ToString() + "/" + healthScript.initialTankHealth.ToString();
        tankSpeedText.text = movementScript.velocityInKMH.ToString("F0") + " km/h";
    }
}
