using UnityEngine;

public class KeyboardInputController : InputController{

	private void Update(){
		GetInput();
	}

	public override void GetInput(){
		DriveInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1, 1);
        TurnInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1, 1);
        BrakeInput = Input.GetKey("space");
	}
}