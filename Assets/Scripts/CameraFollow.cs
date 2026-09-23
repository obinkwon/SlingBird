using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private bool hasSnapped;

    private void LateUpdate()
    {
        if (target == null)
            return;

        // 첫 LateUpdate 프레임에서만 즉시 스냅.
        // 모든 오브젝트의 Start()는 이 시점 이전에 이미 끝나 있으므로
        // (Awake 전체 -> Start 전체 -> Update 전체 -> LateUpdate 전체),
        // StageManager가 플레이어를 시작 플랫폼으로 옮긴 "이후"의
        // 위치를 항상 보게 되어 스크립트 실행 순서에 영향받지 않는다.
        if (!hasSnapped)
        {
            transform.position = GetTargetPosition();
            hasSnapped = true;
            return;
        }

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
