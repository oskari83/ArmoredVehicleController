using UnityEngine;
using MMV;

public class CameraFollower : MonoBehaviour
{
    public MMV_MBT_Vehicle vehicleTarget;

    [Header("Position relative to the vehicle")]
    public Vector3 cameraPosition;
    public float moveSpeed;

    private Vector3 currentOffset;

    void FixedUpdate()
    {
        var _vehicleCannon = vehicleTarget.Turret.Gun;
        var _vehicle = vehicleTarget.transform;

        // camera rotation

        // look between the front of the cannon and the vehicle
        var _bounds = new Bounds(_vehicle.transform.position, Vector3.zero);
        _bounds.Encapsulate(vehicleTarget.transform.position);
        _bounds.Encapsulate(vehicleTarget.Turret.Gun.transform.position + (_vehicleCannon.forward * 30));
        var _cameraRot = Quaternion.LookRotation(_bounds.center);

        // camera position

        var _right = vehicleTarget.transform.right * cameraPosition.x;
        var _up = vehicleTarget.transform.up * cameraPosition.y;
        var _forward = vehicleTarget.transform.forward * cameraPosition.z;
        var _cameraOffset = _right + _up + _forward;

        currentOffset = Vector3.Lerp(currentOffset, _cameraOffset, Time.deltaTime * moveSpeed);
        var _cameraPos = vehicleTarget.transform.position + currentOffset;

        // ---

        transform.position = _cameraPos;
        transform.LookAt(_bounds.center);
    }
}
