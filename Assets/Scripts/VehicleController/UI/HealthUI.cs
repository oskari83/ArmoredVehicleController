using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour{

    private VehicleControllerManager vehicleManager;
    private Armor healthScript;
    public GameObject healthBarObject;
    public GameObject UICanvasObject;
    private GameObject health_bar;
    private Image health_bar_image;
    private Text health_bar_text;

    private void Start(){
        vehicleManager = GetComponent<VehicleControllerManager>();
        healthScript = GetComponent<Armor>();
        health_bar = Instantiate(healthBarObject, UICanvasObject.transform);
        health_bar_image = health_bar.transform.GetChild(1).gameObject.GetComponent<Image>();
        health_bar_text = health_bar.transform.GetChild(4).gameObject.GetComponent<Text>();
    }

    private void Update(){
        health_bar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 5f, 0f));
        health_bar_image.fillAmount = healthScript.tankHealth / healthScript.initialTankHealth;
        health_bar_text.text = healthScript.tankHealth.ToString() + "/" + healthScript.initialTankHealth.ToString();
    }
}
