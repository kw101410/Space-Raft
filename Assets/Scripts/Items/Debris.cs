using UnityEngine;

public class Debris : MonoBehaviour
{
    public ItemData itemData; // 이제 Enum 대신 실제 아이템 데이터를 넣습니다!
    private Vector3 moveDirection;
    private float moveSpeed;
    private float rotationSpeed;
    private Vector3 rotationAxis;
    private float lifeDistance = 50f; // 이 거리보다 멀어지면 자동 삭제

    public void Initialize(Vector3 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        // 우주 쓰레기답게 무작위 회전 추가
        rotationSpeed = Random.Range(10f, 50f);
        rotationAxis = Random.onUnitSphere;
    }

    private void Update()
    {
        // 일정 방향으로 이동
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // 자가 회전 (우주 공간 느낌)
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);

        // 플레이어(원점)로부터 너무 멀어지면 자동 삭제 (최적화)
        if (Vector3.Distance(transform.position, Vector3.zero) > lifeDistance)
        {
            Destroy(gameObject);
        }
    }
}
