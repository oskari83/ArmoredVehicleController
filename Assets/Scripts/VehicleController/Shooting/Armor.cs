using UnityEngine;

public class Armor : MonoBehaviour{

    public float tankHealth = 1000f;
    public float initialTankHealth;
    private void Start(){
        initialTankHealth = tankHealth;
    }

    private void Update(){
        
    }

    public void TakeHit(float _damage, float _angle, string _tank){
        if(_angle < 45f) {
            TakeDamage(_damage);
            Debug.Log("penetrated for: " + _damage.ToString() + " damage, by: " + _tank);
        } else {
            Debug.Log("bounced");
        }
    }

    private void TakeDamage(float _amount) {
        tankHealth -= _amount;
    }
}
