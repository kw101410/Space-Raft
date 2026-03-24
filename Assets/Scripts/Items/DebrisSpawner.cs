using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] debrisPrefabs; // 생성할 쓰레기 프리팹들
    public float minSpawnRate = 1f;
    public float maxSpawnRate = 3f;
    public float spawnDistance = 30f; // 플레이어로부터 얼마나 떨어진 곳에서 생성할지
    public float spawnWidth = 20f;    // 생성되는 가로 범위
    public float spawnHeight = 15f;   // 생성되는 세로(상하) 범위 추가!

    [Header("Movement Settings")]
    public Vector3 driftDirection = new Vector3(0, 0, -1); // 기본적으로 플레이어 쪽으로 흘러오게 설정
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    private void Start()
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0)
        {
            Debug.LogError("Debris Prefabs가 설정되지 않았습니다! 프리팹을 할당해 주세요.");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnRate, maxSpawnRate);
            yield return new WaitForSeconds(waitTime);

            SpawnDebris();
        }
    }

    private void SpawnDebris()
    {
        // 1. 생성 위치 계산 (플레이어 앞쪽 일정 거리 + 무작위 가로/세로 위치)
        Vector3 spawnCenter = -driftDirection.normalized * spawnDistance;
        
        // 가로 방향(Side)과 세로 방향(Up) 벡터 계산
        Vector3 side = Vector3.Cross(driftDirection, Vector3.up).normalized;
        if (side == Vector3.zero) side = Vector3.right; // 방향이 수직일 경우 예외 처리
        Vector3 up = Vector3.Cross(side, driftDirection).normalized;

        float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        float randomY = Random.Range(-spawnHeight / 2f, spawnHeight / 2f);
        
        Vector3 finalSpawnPos = spawnCenter + (side * randomX) + (up * randomY);

        // 2. 랜덤 프리팹 선택 및 생성
        int randomIndex = Random.Range(0, debrisPrefabs.Length);
        GameObject debrisObj = Instantiate(debrisPrefabs[randomIndex], finalSpawnPos, Random.rotation);

        // 3. 자원 레이어 설정 (필요 시 코드에서 강제 지정)
        // debrisObj.layer = LayerMask.NameToLayer("Collectible");

        // 4. Debris 컴포넌트 추가 및 초기화
        Debris debrisScript = debrisObj.AddComponent<Debris>();
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        debrisScript.Initialize(driftDirection, randomSpeed);
    }

    // 에디터 뷰에서 스폰 영역을 시각적으로 확인하기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 spawnCenter = -driftDirection.normalized * spawnDistance;
        
        Vector3 side = Vector3.Cross(driftDirection, Vector3.up).normalized;
        if (side == Vector3.zero) side = Vector3.right;
        Vector3 up = Vector3.Cross(side, driftDirection).normalized;

        // 3D 사각형 영역 시각화 (기즈모)
        Vector3 topLeft = spawnCenter + (up * spawnHeight / 2f) - (side * spawnWidth / 2f);
        Vector3 topRight = spawnCenter + (up * spawnHeight / 2f) + (side * spawnWidth / 2f);
        Vector3 bottomLeft = spawnCenter - (up * spawnHeight / 2f) - (side * spawnWidth / 2f);
        Vector3 bottomRight = spawnCenter - (up * spawnHeight / 2f) + (side * spawnWidth / 2f);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
