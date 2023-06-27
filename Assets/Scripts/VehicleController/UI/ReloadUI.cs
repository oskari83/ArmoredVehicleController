using UnityEngine;
using UnityEngine.UI;
using System;

public class ReloadUI : MonoBehaviour{
    
    public GameObject reloadTimerTextGameObject;
    public GameObject fixedCrosshairBlueFillLeft;
    public GameObject fixedCrosshairBlueFillRight;
    
    private ShootingController shootingController;
    private Text reloadTimerText;
    private Outline outlineReloadtext;
    private Image blueFillLeft;
    private Image blueFillRight;

    private void Awake(){
        shootingController = gameObject.GetComponent<ShootingController>();
        blueFillLeft = fixedCrosshairBlueFillLeft.GetComponent<Image>();
        blueFillRight = fixedCrosshairBlueFillRight.GetComponent<Image>();
        blueFillLeft.color = UIColours.blue;
        blueFillRight.color = UIColours.blue;
        reloadTimerText = reloadTimerTextGameObject.GetComponent<Text>();
        outlineReloadtext = reloadTimerTextGameObject.GetComponent<Outline>();
    }

    private void Update(){
        double roundedValue = Math.Round(shootingController.reloadTimeLeft, 2);
        string formattedString = string.Format("{0:0.00}", roundedValue);
        reloadTimerText.text = $"{formattedString}s";
        if(shootingController.isReloading){
            outlineReloadtext.enabled = false;
            reloadTimerText.color = UIColours.blackSelected;
            float fillAmountBasedOnReloadLeft = 1f - (shootingController.reloadTimeLeft / shootingController.currentMaxReload);
            blueFillLeft.fillAmount = fillAmountBasedOnReloadLeft;
            blueFillRight.fillAmount = fillAmountBasedOnReloadLeft;
        }else{
            outlineReloadtext.enabled = true;
            reloadTimerText.color = UIColours.blue;
            blueFillLeft.fillAmount = 0f;
            blueFillRight.fillAmount = 0f;
        }
    }
}
