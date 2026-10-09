// using System.ComponentModel.DataAnnotations;
// using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using System.Numerics;
using System.Diagnostics;
// using System.Threading.Tasks.Dataflow;

/*
 * SplineFollow rides a SplinePath. Each frame it moves along the path,
 * finds u, places itself on the curve, and faces the target or the tangent.
 */

public class SplineFollow : MonoBehaviour
{
    public SplinePath path;
    public RocketLaunch rocketLaunch;
    public Transform target;
    public float speed = 2.5f; // Positive world units per second in the completed exercise.
    public bool travelByDistance = true;
    public bool faceTarget = true;
    public  bool rocket;

    float _distance;
    float _u;
    UnityEngine.Vector3 prevPosition;

    void Awake()
    {
        Restart();
        prevPosition = new UnityEngine.Vector3(0f, 0f, 0f);
    }
    
    void Update()
    {
        if (travelByDistance)
        {
            // TODO: Advance distance by speed over the frame and look up u for that distance.
            // Stop at TotalLength.
            _distance = math.min(_distance + speed * Time.deltaTime, path.TotalLength);
            _u = path.ParameterAtDistance(_distance);
        }
        else
        {
            // TODO: Advance u in equal steps, paced so the trip takes as long as the distance
            // trip at the same speed. Stop at SegmentCount.
            _u = math.min(_u + speed * (path.SegmentCount / path.TotalLength) * Time.deltaTime, path.SegmentCount);
        }

        // TODO: Place this object at the path point for u. Replay should return it to the start.
        transform.position = path.SamplePoint(_u);

        // TODO: Look at the target if faceTarget is on, otherwise along the path tangent.
        // Use world up so the horizon stays level.
        if(faceTarget)
            transform.LookAt(target);
        else
        {
            // transform.LookAt(path.SampleTangent(_u), UnityEngine.Vector3.up);
            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.LookRotation(path.SamplePoint(_u), UnityEngine.Vector3.up);
        }

        if (rocket == true && prevPosition == transform.position)
            rocketLaunch.Launch();

        prevPosition = transform.position;
            
    }

    public void Restart()
    {
        _distance = 0f;
        _u = 0f;
    }

}
