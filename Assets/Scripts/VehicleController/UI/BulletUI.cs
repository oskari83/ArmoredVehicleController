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

    private void Start(){
        shootingController = gameObject.GetComponent<ShootingController>();
        InitializeBulletSelectedUI();
        UpdateBulletSelectedUI();
    }

    private void Update(){
        if (Input.GetKey(KeyCode.Alpha1)){
            shootingController.selectedBullet = 0;
            UpdateBulletSelectedUI();
        }

        if (Input.GetKey(KeyCode.Alpha2)){
            shootingController.selectedBullet = 1;
            UpdateBulletSelectedUI();
        }

        if (Input.GetKey(KeyCode.Alpha3)){
            shootingController.selectedBullet = 2;
            UpdateBulletSelectedUI();
        }
    }

    public void UpdateBulletCountUI(){
        if(shootingController.selectedBullet==0){
            bultype1.text = $"AP - {shootingController.bulletsOfT[shootingController.selectedBullet]}";
        }else if(shootingController.selectedBullet==1){
            bultype2.text = $"HV - {shootingController.bulletsOfT[shootingController.selectedBullet]}";
        }else{
            bultype3.text = $"HE - {shootingController.bulletsOfT[shootingController.selectedBullet]}";
        }
    }

    private void InitializeBulletSelectedUI(){
        bultype1.text = $"AP - {shootingController.bulletsOfT[0]}";
        bultype2.text = $"HV - {shootingController.bulletsOfT[1]}";
        bultype3.text = $"HE - {shootingController.bulletsOfT[2]}";
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