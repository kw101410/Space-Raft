using UnityEngine;
using UnityEngine.InputSystem;

public class HookSystem : MonoBehaviour
{
    [Header("Hook Settings")]
    public Transform cameraTransform;
    public float maxDistance = 30f;
    public float pullSpeed = 10f;
    public LayerMask collectibleLayer;

    [Header("Visual Settings")]
    public LineRenderer lineRenderer;
    public float ropeWidth = 0.05f;
    public Color crosshairColor = Color.white; // 에임 점 색상 조절용

    private bool isHooked = false;
    private Transform hookedObject;
    private Vector3 hookPoint;

    private InputAction fireAction;

    private void OnGUI()
    {
        // 화면 정중앙에 4x4 픽셀 크기의 점 찍기 (에이미!)
        int size = 4;
        float posX = Screen.width / 2 - size / 2;
        float posY = Screen.height / 2 - size / 2;

        Texture2D texture = Texture2D.whiteTexture;
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(posX, posY, size, size), texture);
    }

    private void Awake()
    {
        // 마우스 왼쪽 클릭 (또는 게임패드 오른쪽 트리거)
        fireAction = new InputAction("Fire", binding: "<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            
            // 기본 하얀색 머티리얼 설정 (분홍색 사각형 방지)
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.gray;
        }
    }

    private void OnEnable()
    {
        fireAction.Enable();
    }

    private void OnDisable()
    {
        fireAction.Disable();
    }

    private void Update()
    {
        if (fireAction.WasPressedThisFrame())
        {
            ThrowHook();
        }

        if (fireAction.IsPressed() && isHooked)
        {
            PullObject();
        }
        else if (fireAction.WasReleasedThisFrame())
        {
            ReleaseHook();
        }

        UpdateRope();
    }

    private void ThrowHook()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, collectibleLayer))
        {
            isHooked = true;
            hookedObject = hit.transform;
            hookPoint = hit.point;
            lineRenderer.enabled = true;
            
            // 물체가 움직이지 않도록 잠시 Kinematic 설정을 하거나 이동 로직을 태울 수 있음
            // 여기서는 단순 이동만 구현
        }
    }

    private void PullObject()
    {
        if (hookedObject == null)
        {
            ReleaseHook();
            return;
        }

        // 플레이어 몸 근처(카메라 위치)로 직접 끌어오도록 목표점 수정
        Vector3 targetPos = transform.position + transform.forward * 0.5f; 
        hookedObject.position = Vector3.MoveTowards(hookedObject.position, targetPos, pullSpeed * Time.deltaTime);

        // 거리가 조금 멀어도(1.5f 이내) 획득 성공하도록 판정을 후하게 줍니다.
        // 그리고 목표 지점을 targetPos가 아니라 플레이어 중심으로 봅니다.
        if (Vector3.Distance(hookedObject.position, transform.position) < 2.0f)
        {
            Debris debris = hookedObject.GetComponent<Debris>();
            if (debris != null)
            {
                if (debris.itemData != null)
                {
                    InventoryManager.Instance.AddItem(debris.itemData, 1);
                    Debug.Log($"[아이템 획득] {debris.itemData.itemName}이(가) 인벤토리에 들어갔습니다.");
                }
                else
                {
                    Debug.LogWarning($"[오류] 가져온 물체({hookedObject.name})의 Debris 성분에 ItemData가 비어있습니다! 유니티 에디터에서 해당 프리팹을 확인하세요.");
                }
            }
            
            Destroy(hookedObject.gameObject);
            ReleaseHook();
        }
    }

    private void ReleaseHook()
    {
        isHooked = false;
        hookedObject = null;
        lineRenderer.enabled = false;
    }

    private void UpdateRope()
    {
        if (isHooked && hookedObject != null)
        {
            // 플레이어 손 위치(혹은 카메라 아래)에서 물체까지 라인 그림
            lineRenderer.SetPosition(0, transform.position + transform.right * 0.3f - transform.up * 0.2f);
            lineRenderer.SetPosition(1, hookedObject.position);
        }
    }
}
