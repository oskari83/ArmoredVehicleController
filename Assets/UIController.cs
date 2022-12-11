using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour{

    public Color blackSelected;
    public Color blackDeselected;

    private ShootingController shoot;

    public Text bultype1;
    public Text bultype2;
    public Text bultype3;
    public RectTransform bultypes;

    private void Start(){
        shoot = gameObject.GetComponent<ShootingController>();
        InitializeBulletSelectedUI();
        UpdateBulletSelectedUI();
    }

    private void Update(){
        if (Input.GetKey(KeyCode.Alpha1)){
            shoot.selectedBullet = 0;
            UpdateBulletSelectedUI();
        }

        if (Input.GetKey(KeyCode.Alpha2)){
            shoot.selectedBullet = 1;
            UpdateBulletSelectedUI();
        }

        if (Input.GetKey(KeyCode.Alpha3)){
            shoot.selectedBullet = 2;
            UpdateBulletSelectedUI();
        }
    }

    public void UpdateBulletCountUI(){
        if(shoot.selectedBullet==0){
            bultype1.text = $"AP - {shoot.bulletsOfT[shoot.selectedBullet]}";
        }else if(shoot.selectedBullet==1){
            bultype2.text = $"HV - {shoot.bulletsOfT[shoot.selectedBullet]}";
        }else{
            bultype3.text = $"HE - {shoot.bulletsOfT[shoot.selectedBullet]}";
        }
    }

    private void InitializeBulletSelectedUI(){
        bultype1.text = $"AP - {shoot.bulletsOfT[0]}";
        bultype2.text = $"HV - {shoot.bulletsOfT[1]}";
        bultype3.text = $"HE - {shoot.bulletsOfT[2]}";
    }

    private void UpdateBulletSelectedUI(){
        if(shoot.selectedBullet==0){
            bultype1.fontSize = 10;
            bultype2.fontSize = 8;
            bultype3.fontSize = 8;
            bultype1.color = blackSelected;
            bultype2.color = blackDeselected;
            bultype3.color = blackDeselected;
            bultypes.localPosition = new Vector3(0,-7.8f,0);
        }else if(shoot.selectedBullet==1){
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
