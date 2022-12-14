using UnityEngine;

public abstract class InputController : MonoBehaviour{

	public float DriveInput { get; set; }
    public float TurnInput { get; set; }
	public bool BrakeInput { get; set; }

	public abstract void GetInput();
}