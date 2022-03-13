using MMV;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAI : MonoBehaviour
{
    public NavMeshAgent agent;

    [Space(10)]
    public float stopWithAngle;
    public float curveSensitive;
    public float detectCurveDistance;

    [Space(10)]
    public float minVelocity;

    private MMV_MBT_Vehicle vehicle;

    private Vector3 targetPos;

    void Start()
    {
        vehicle = GetComponent<MMV.MMV_MBT_Vehicle>();
        targetPos = GetRandomPoint();
    }

    void Update()
    {
        var steeringTarget = agent.steeringTarget;

        // control the vehicle
        vehicle.MoveTo(agent, targetPos, curveSensitive, vehicle.Engine.MaxForwardVelocity, stopWithAngle, minVelocity);
        vehicle.TurretTargetPosition = steeringTarget;
        

        var path = agent.path.corners;

        // draw AI path
        for (int i = 0; i < path.Length - 1; i++)
        {
            Debug.DrawRay(path[i], Vector3.up * 5, Color.red);

            if (i + 1 <= path.Length - 1)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.blue);
            }
        }

        // generate new random target position
        if (Vector3.Distance(transform.position, targetPos) < 10)
        {
            targetPos = GetRandomPoint();
        }

        // generates a new path if the current one is inaccessible
        if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            targetPos = GetRandomPoint();
        }
    }

    // get random positions on navmesh
    private Vector3 GetRandomPoint()
    {
        var randomPoint = new Vector3();

        randomPoint = Random.insideUnitSphere * 300;
        randomPoint += transform.position;

        NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 300, 1);
        randomPoint = hit.position;

        return randomPoint;
    }
}
