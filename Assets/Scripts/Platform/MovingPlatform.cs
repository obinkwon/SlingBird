using UnityEngine;

public class MovingPlatform : Platform
{
    [Header("Moving Settings")]
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float x =
            Mathf.Sin(Time.time * moveSpeed) *
            moveDistance;

        transform.position =
            startPosition +
            Vector3.right * x;
    }
}