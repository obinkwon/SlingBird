using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Tooltip("현재 목표를 얻어오기 위한 StageManager. 비워두면 자동으로 찾습니다.")]
    [SerializeField] private StageManager stageManager;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Goal Framing")]
    [Tooltip("당기는 동안 플레이어와 목표가 함께 보이도록 줌아웃한다.")]
    [SerializeField] private bool frameGoal = true;

    [Tooltip("플레이어/목표 주변에 남길 여백 (월드 단위)")]
    [SerializeField] private float framePadding = 2f;

    [Tooltip("줌아웃 상한 (orthographicSize)")]
    [SerializeField] private float maxOrthographicSize = 14f;

    [Tooltip("당길 때 목표가 가까워도 최소한 이 배율만큼은 넓게 보여준다.")]
    [SerializeField] private float aimZoomMultiplier = 1.3f;

    private Camera cam;
    private PlayerController.PlayerState lastState = PlayerController.PlayerState.Ready;
    private PlayerController player;
    private float baseOrthographicSize;

    private Vector3 desiredPosition;
    private float desiredSize;

    private bool hasSnapped;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;
        desiredSize = baseOrthographicSize;

        if (stageManager == null)
            stageManager = FindFirstObjectByType<StageManager>();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (player == null)
            player = target.GetComponent<PlayerController>();

        UpdateDesired();

        // 첫 LateUpdate 프레임에서만 즉시 스냅.
        // 모든 오브젝트의 Start()는 이 시점 이전에 이미 끝나 있으므로
        // (Awake 전체 -> Start 전체 -> Update 전체 -> LateUpdate 전체),
        // StageManager가 플레이어를 시작 플랫폼으로 옮긴 "이후"의
        // 위치를 항상 보게 되어 스크립트 실행 순서에 영향받지 않는다.
        if (!hasSnapped)
        {
            transform.position = desiredPosition;
            cam.orthographicSize = desiredSize;
            hasSnapped = true;
            return;
        }

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, t);
    }

    private void UpdateDesired()
    {
        PlayerController.PlayerState state =
            player != null ? player.State : PlayerController.PlayerState.Flying;

        bool aiming = state == PlayerController.PlayerState.Aiming;
        bool wasAiming = lastState == PlayerController.PlayerState.Aiming;
        lastState = state;

        // 당기기 시작한 순간에만 프레이밍을 계산하고 조준 중에는 고정한다.
        // (드래그 중 카메라가 계속 움직이면 마우스 월드 좌표가 흔들림)
        if (aiming)
        {
            if (!wasAiming)
                FrameGoal();
            return;
        }

        // 놓은 뒤(비행/대기/사망): 플레이어를 따라가며 기본 줌으로 복귀
        desiredSize = baseOrthographicSize;
        desiredPosition = target.position + offset;
        desiredPosition.z = offset.z;
    }

    // 당기는 동안 플레이어와 목표가 함께 보이도록 줌아웃한다.
    private void FrameGoal()
    {
        Collider2D goal = GetGoalCollider();

        if (!frameGoal || goal == null)
            return;

        Bounds bounds = goal.bounds;
        bounds.Encapsulate(target.position);
        bounds.Expand(framePadding * 2f);

        float sizeForHeight = bounds.extents.y;
        float sizeForWidth = bounds.extents.x / cam.aspect;

        float maxSize = Mathf.Max(baseOrthographicSize, maxOrthographicSize);

        desiredSize = Mathf.Clamp(
            Mathf.Max(sizeForHeight, sizeForWidth, baseOrthographicSize * aimZoomMultiplier),
            baseOrthographicSize,
            maxSize);

        desiredPosition = new Vector3(bounds.center.x, bounds.center.y, offset.z);
    }

    private Collider2D GetGoalCollider()
    {
        if (stageManager == null || stageManager.CurrentGoal == null)
            return null;

        return stageManager.CurrentGoal.GetComponent<Collider2D>();
    }
}
