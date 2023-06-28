using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class BulletUI : MonoBehaviour{

    public GameObject bulletChoiceAmountGameObject;
    public GameObject clipBulletParentObject;
    public GameObject clipBulletIconPrefab;

    private Text bulletChoiceAmountText;
    private Outline bulletTextOutline;
	private ShootingController shootingController;

    private List<Image> clipBulletUIList = new List<Image>();

    private void Awake(){
        shootingController = gameObject.GetComponent<ShootingController>();
        bulletChoiceAmountText = bulletChoiceAmountGameObject.GetComponent<Text>();
        bulletTextOutline = bulletChoiceAmountGameObject.GetComponent<Outline>();
        UpdateBulletSelectedUI();

        if(shootingController.hasClip){
            for(int i=0; i<shootingController.clipSize; i++){
                Image bulletUI = Instantiate(clipBulletIconPrefab, clipBulletParentObject.transform).GetComponent<Image>();
                bulletUI.color = UIColours.blackDeselected;
                clipBulletUIList.Add(bulletUI);
            }
        }
    }

    private void Update(){
        if(shootingController.isReloading){
            bulletTextOutline.enabled = false;
            bulletChoiceAmountText.color = UIColours.blackSelected;
        }else{
            bulletTextOutline.enabled = true;
            bulletChoiceAmountText.color = UIColours.blue;
        }

        if(shootingController.finishedFullReload){
            if(shootingController.hasClip){
                UpdateClipBulletsOnReload();
            }
            shootingController.finishedFullReload = false;
        }
    }

    private void OnEnable() {
        shootingController.shootingEvent += UpdateBulletCountUI;
        shootingController.bulletSwitchEvent += UpdateBulletSelectedUI;
        shootingController.fullReloadEvent += EmptyClipBulletUI;
    }

    private void OnDisable() {
        shootingController.shootingEvent -= UpdateBulletCountUI;
        shootingController.bulletSwitchEvent -= UpdateBulletSelectedUI;
        shootingController.fullReloadEvent -= EmptyClipBulletUI;
    }

    public void UpdateBulletCountUI(){
        if(shootingController.selectedBullet==0){
            bulletChoiceAmountText.text = $"AP - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else if(shootingController.selectedBullet==1){
            bulletChoiceAmountText.text = $"HV - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else{
            bulletChoiceAmountText.text = $"HE - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }

        if(shootingController.hasClip){
            UpdateClipBulletUI();
        }
    }

    private void UpdateBulletSelectedUI(){
        if(shootingController.selectedBullet==0){
            bulletChoiceAmountText.text = $"AP - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else if(shootingController.selectedBullet==1){
            bulletChoiceAmountText.text = $"HV - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }else{
            bulletChoiceAmountText.text = $"HE - [{shootingController.bulletCountOfType[shootingController.selectedBullet]}]";
        }
    }

    private void UpdateClipBulletUI(){
        int clipShootingIndex = shootingController.clipSize - 1 - shootingController.currentAmmunition;
        clipBulletUIList[clipShootingIndex].color = UIColours.blackDeselected;
    }

    private void EmptyClipBulletUI() {
        if(shootingController.hasClip){
            for (int i = 0; i < shootingController.clipSize; i++){
                clipBulletUIList[i].color = UIColours.blackDeselected;
            }
        }
    }

    private void UpdateClipBulletsOnReload(){
        for (int i = 0; i < shootingController.clipSize; i++){
            clipBulletUIList[i].color = UIColours.blue;
        }
    }
}