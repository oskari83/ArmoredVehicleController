using UnityEngine;
using UnityEngine.UI;
using System;

public class ReloadUI : MonoBehaviour{

    public Text reloadTimerText;
    public Color blue;
    public Color black;
    
    private ShootingController shootingController;

    private void Awake(){
        shootingController = gameObject.GetComponent<ShootingController>();
    }

    private void Update(){
        double roundedValue = Math.Round(shootingController.reloadTimeLeft, 2);
        string formattedString = string.Format("{0:0.00}", roundedValue);
        reloadTimerText.text = $"{formattedString}s";
        if(shootingController.isReloading){
            reloadTimerText.color = black;
        }else{
            reloadTimerText.color = blue;
        }
    }
}
