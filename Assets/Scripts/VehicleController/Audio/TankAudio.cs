using UnityEngine;

public class TankAudio : MonoBehaviour{

	public AudioSource engineAudioSource;
	public float maxPitch = 1.2f;
    public float idlePitch = 0.4f;
	private float currentPitch = 0.5f;
	private TankMovement tankMovementScript;
	private Rigidbody rigidBody;

	private void Awake(){
		rigidBody = this.GetComponent<Rigidbody>();
	}

	private void LateUpdate(){
		PlayEngineAudio();
	}

	private void PlayEngineAudio(){
        currentPitch = Mathf.Clamp(idlePitch + rigidBody.velocity.magnitude / 40.0f, idlePitch, maxPitch);
        engineAudioSource.pitch = currentPitch;
	}
}