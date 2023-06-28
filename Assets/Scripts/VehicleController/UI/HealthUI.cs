using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour{

    private VehicleControllerManager vehicleManager;
    private Camera camera;
    public Armor healthScript;
    public GameObject healthBarObject;
    public GameObject UICanvasObject;
    private GameObject health_bar;
    public Image health_bar_image;
    public Text health_bar_text;

    private void Start(){
        vehicleManager = Camera.main.gameObject.GetComponent<CameraController>().vehicleManager;
        healthScript = GetComponent<Armor>();
        health_bar = Instantiate(healthBarObject, UICanvasObject.transform);
        health_bar_image = health_bar.transform.GetChild(1).gameObject.GetComponent<Image>();
        health_bar_text = health_bar.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Text>();
    }

    private void LateUpdate(){
        camera = vehicleManager.CameraInUse;
        Vector3 screenPointFromCamera = camera.WorldToScreenPoint(transform.position + new Vector3(0f, 5f, 0f));
        if(screenPointFromCamera.z < 0f) {
            health_bar.SetActive(false);
        } else {
            health_bar.SetActive(true);
            health_bar.transform.position = screenPointFromCamera;
        }

        health_bar_image.fillAmount = healthScript.tankHealth / healthScript.initialTankHealth;
        health_bar_text.text = healthScript.tankHealth.ToString() + "/" + healthScript.initialTankHealth.ToString();
    }
}
