using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private void Start()
    {
        if (target != null)
            transform.position = GetTargetPosition();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, GetTargetPosition(), t);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 pos = target.position + offset;
        pos.z = offset.z;
        return pos;
    }
}
