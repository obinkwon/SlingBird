using System.Collections;
using UnityEngine;

public class BreakPlatform : Platform
{
    [Header("Break Settings")]
    [SerializeField] private float breakDelay = 0.5f;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeAmount = 0.08f;

    private bool isBreaking;

    protected override void OnPlayerLanded(PlayerController player)
    {
        if (isBreaking)
            return;

        // 기존 플랫폼의 착지 처리
        base.OnPlayerLanded(player);

        isBreaking = true;

        StartCoroutine(BreakRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        yield return new WaitForSeconds(breakDelay);

        Vector3 originalPosition =
            transform.position;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 randomOffset =
                Random.insideUnitCircle * shakeAmount;

            transform.position =
                originalPosition + randomOffset;

            yield return null;
        }

        transform.position = originalPosition;

        Destroy(gameObject);
    }
}