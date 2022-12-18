using UnityEngine;
using UnityEngine.UI;

public class BulletUI : MonoBehaviour{

    public GameObject bulletChoiceAmountGameObject;

    private Text bulletChoiceAmountText;
    private Outline bulletTextOutline;
	private ShootingController shootingController;

    private void Awake(){
        shootingController = gameObject.GetComponent<ShootingController>();
        bulletChoiceAmountText = bulletChoiceAmountGameObject.GetComponent<Text>();
        bulletTextOutline = bulletChoiceAmountGameObject.GetComponent<Outline>();
        UpdateBulletSelectedUI();
    }

    private void Update(){
        if(shootingController.isReloading){
            bulletTextOutline.enabled = false;
            bulletChoiceAmountText.color = UIColours.blackSelected;
        }else{
            bulletTextOutline.enabled = true;
            bulletChoiceAmountText.color = UIColours.blue;
        }
    }

    private void OnEnable() {
        shootingController.shootingEvent += UpdateBulletCountUI;
        shootingController.bulletSwitchEvent += UpdateBulletSelectedUI;
    }

    private void OnDisable() {
        shootingController.shootingEvent -= UpdateBulletCountUI;
        shootingController.bulletSwitchEvent -= UpdateBulletSelectedUI;
    }

    public void UpdateBulletCountUI(){
        if(shootingController.selectedBullet==0){
            bulletChoiceAmountText.text = $"AP - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else if(shootingController.selectedBullet==1){
            bulletChoiceAmountText.text = $"HV - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else{
            bulletChoiceAmountText.text = $"HE - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }
    }

    private void UpdateBulletSelectedUI(){
        // Do something
        if(shootingController.selectedBullet==0){
            bulletChoiceAmountText.text = $"AP - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else if(shootingController.selectedBullet==1){
            bulletChoiceAmountText.text = $"HV - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else{
            bulletChoiceAmountText.text = $"HE - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }
    }
}