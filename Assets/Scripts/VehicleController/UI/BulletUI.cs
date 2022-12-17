using UnityEngine;
using UnityEngine.UI;

public class BulletUI : MonoBehaviour{

	public Color blackSelected;
    public Color blackDeselected;

    public Text bultype1;
    public Text bultype2;
    public Text bultype3;
    public RectTransform bultypes;

	private ShootingController shootingController;

    private void Awake(){
        shootingController = gameObject.GetComponent<ShootingController>();
        InitializeBulletSelectedUI();
        UpdateBulletSelectedUI();
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
            bultype1.text = $"AP - {shootingController.bulletCountOfType[shootingController.selectedBullet]}";
        }else if(shootingController.selectedBullet==1){
            bultype2.text = $"HV - {shootingController.bulletCountOfType[shootingController.selectedBullet]}";
        }else{
            bultype3.text = $"HE - {shootingController.bulletCountOfType[shootingController.selectedBullet]}";
        }
    }

    private void InitializeBulletSelectedUI(){
        bultype1.text = $"AP - {shootingController.bulletCountOfType[0]}";
        bultype2.text = $"HV - {shootingController.bulletCountOfType[1]}";
        bultype3.text = $"HE - {shootingController.bulletCountOfType[2]}";
    }

    private void UpdateBulletSelectedUI(){
        if(shootingController.selectedBullet==0){
            bultype1.fontSize = 10;
            bultype2.fontSize = 8;
            bultype3.fontSize = 8;
            bultype1.color = blackSelected;
            bultype2.color = blackDeselected;
            bultype3.color = blackDeselected;
            bultypes.localPosition = new Vector3(0,-7.8f,0);
        }else if(shootingController.selectedBullet==1){
            bultype1.fontSize = 8;
            bultype2.fontSize = 10;
            bultype3.fontSize = 8;
            bultype1.color = blackDeselected;
            bultype2.color = blackSelected;
            bultype3.color = blackDeselected;
            bultypes.localPosition = new Vector3(0,0f,0);
        }else{
            bultype1.fontSize = 8;
            bultype2.fontSize = 8;
            bultype3.fontSize = 10;
            bultype1.color = blackDeselected;
            bultype2.color = blackDeselected;
            bultype3.color = blackSelected;
            bultypes.localPosition = new Vector3(0,7.8f,0);
        }
    }
}