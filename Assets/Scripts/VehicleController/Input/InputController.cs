using UnityEngine;

public abstract class InputController : MonoBehaviour{

	public float DriveInput { get; set; }
    public float TurnInput { get; set; }
	public bool BrakeInput { get; set; }

	public float MouseXInput { get; set; }
	public float MouseYInput { get; set; }
	public float MouseScrollInput { get; set; }

	public abstract void GetInput();
}