using UnityEngine;

public class MouseInputController : InputController{
    private void Update(){
		GetInput();
	}

	public override void GetInput(){
		MouseXInput = Input.GetAxis("Mouse X");
        MouseYInput = Input.GetAxis("Mouse Y");
        MouseScrollInput = Input.GetAxis("Mouse ScrollWheel");
	}
}
