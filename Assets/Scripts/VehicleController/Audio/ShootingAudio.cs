using UnityEngine;

public class ShootingAudio : MonoBehaviour{

	[SerializeField] private AudioSource _shootAudioSource;
    private ShootingController _shootingController;

    private void Awake(){
        _shootingController = GetComponent<ShootingController>();
    }

    private void OnEnable() {
        _shootingController.shootingEvent += PlayFireSound;
    }

    private void OnDisable() {
        _shootingController.shootingEvent -= PlayFireSound;
    }

	private void PlayFireSound(){
        _shootAudioSource.Play();
	}
}