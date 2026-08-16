using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

//AnanDEV || https://youtu.be/gzPjH7-gHLE?si=_iVPKwD79NOsvyoz || https://github.com/AnanD3V/Splines-2.0
public class MoveAlongSpline : MonoBehaviour
{
   [SerializeField] private SplineContainer spline;
   [SerializeField] private float speed = 50f;
   private float distancePercentage = 0f;
   private float splineLength;


    private void Start()
    {
        CaclulateSplineLength(spline);
    }

    private void Update()
    {
        distancePercentage += speed * Time.deltaTime / splineLength;

        Vector3 currentPosition = spline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        if (distancePercentage > 1f)
        {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = spline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }

    public void CaclulateSplineLength(SplineContainer spline)
    {
        splineLength = spline.CalculateLength();
    }

    public void ChangeSpline(SplineContainer newSpline)
    {
        spline = newSpline;
        distancePercentage = 0f;
        CaclulateSplineLength(spline);
    }

    public void ChangeSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}