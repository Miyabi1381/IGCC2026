using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("--- FOLLOW TARGET ---")]
    public Transform Target; // set automatically by DeathManager on spawn, but can be pre-assigned for testing

    [Header("--- FOLLOW SETTINGS ---")]
    public Vector3 Offset = new Vector3(0f, 0f, -10f); // keep Z negative so the camera stays in front in 2D
    public float SmoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;

    public void SetTarget(Transform newTarget)
    {
        Target = newTarget;
    }

    void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desiredPosition = Target.position + Offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, SmoothTime);
    }
}