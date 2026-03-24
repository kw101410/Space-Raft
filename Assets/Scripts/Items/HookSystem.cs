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

        // 플레이어 근처(카메라 위치 아래쪽 등)로 물체를 끌어옴
        Vector3 targetPos = transform.position + transform.forward * 1.5f; 
        hookedObject.position = Vector3.MoveTowards(hookedObject.position, targetPos, pullSpeed * Time.deltaTime);

        // 너무 가까워지면 자동 해제 및 획득 처리
        if (Vector3.Distance(hookedObject.position, targetPos) < 0.2f)
        {
            Debris debris = hookedObject.GetComponent<Debris>();
            if (debris != null && debris.itemData != null)
            {
                InventoryManager.Instance.AddItem(debris.itemData, 1);
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
