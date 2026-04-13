using UnityEngine;
using UnityEngine.InputSystem;

public class HookSystem : MonoBehaviour
{
    private enum HookState { Idle, Thrown, Returning }

    [Header("Hook Settings")]
    public Transform cameraTransform;
    public float maxDistance = 30f;
    public float throwSpeed = 25f; // 갈고리가 날아가는 속도
    public float pullSpeed = 15f;  // 돌아오는 속도
    public float hookHitRadius = 0.5f; // 갈고리 판정 크기 (넉넉하게 설정)
    public LayerMask collectibleLayer;

    [Header("Visual Settings")]
    public LineRenderer lineRenderer;
    public float ropeWidth = 0.05f;
    public Color crosshairColor = Color.white;

    private HookState currentState = HookState.Idle;
    private Transform hookedObject;
    private GameObject thrownHookModel;
    private GameObject hookRootModel; // 현재 장착된 훅의 전체 파츠 (최상위)
    private Transform headParent;     // Head의 원래 부모 위치
    private Vector3 originalHeadLocalPos;
    private Quaternion originalHeadLocalRot;
    private Vector3 targetThrowPos;

    private InputAction fireAction;
    private PlayerEquip equipSys;

    private void OnGUI()
    {
        int size = 4;
        float posX = Screen.width / 2 - size / 2;
        float posY = Screen.height / 2 - size / 2;
        Texture2D texture = Texture2D.whiteTexture;
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(posX, posY, size, size), texture);
    }

    private void Awake()
    {
        equipSys = GetComponent<PlayerEquip>();
        if (equipSys == null) equipSys = FindObjectOfType<PlayerEquip>();

        fireAction = new InputAction("Fire", binding: "<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.gray;
        }
    }

    private void OnEnable() => fireAction.Enable();
    private void OnDisable() => fireAction.Disable();

    private void Update()
    {
        ItemData currentHeldItem = equipSys != null ? equipSys.GetEquippedItem() : null;
        bool canUseHook = currentHeldItem != null && currentHeldItem.itemType == ItemType.Tool;

        // 중간에 다른 아이템으로 스위칭 하는 경우, 훅 강제 회수
        if (currentState != HookState.Idle && (!canUseHook || equipSys.currentEquippedModel == null || hookRootModel != equipSys.currentEquippedModel))
        {
            ResetHookImmediatly();
            return;
        }

        switch (currentState)
        {
            case HookState.Idle:
                if (fireAction.WasPressedThisFrame() && canUseHook)
                {
                    StartThrowing();
                }
                break;

            case HookState.Thrown:
                UpdateThrowing();
                break;

            case HookState.Returning:
                UpdateReturning();
                break;
        }

        UpdateRope();
    }

    private void StartThrowing()
    {
        currentState = HookState.Thrown;
        hookRootModel = equipSys.currentEquippedModel;
        
        // "Head" 자식을 찾아 날아갈 물체로 지정 (나머지 몸체는 플레이어 손에 남음)
        Transform headTransform = hookRootModel.transform.Find("Head");
        if (headTransform != null)
        {
            thrownHookModel = headTransform.gameObject;
            headParent = headTransform.parent;
            originalHeadLocalPos = headTransform.localPosition;
            originalHeadLocalRot = headTransform.localRotation;
        }
        else
        {
            thrownHookModel = hookRootModel;
            headParent = hookRootModel.transform.parent;
            originalHeadLocalPos = hookRootModel.transform.localPosition;
            originalHeadLocalRot = hookRootModel.transform.localRotation;
        }
        
        // 플레이어 카메라 기준 앞으로 날아갈 목표 좌표 계산
        targetThrowPos = cameraTransform.position + cameraTransform.forward * maxDistance;
        
        // 오브젝트를 부모에서 분리하여 독립적으로 날아가게 만듦
        thrownHookModel.transform.SetParent(null);
        
        lineRenderer.enabled = true;
    }

    private void UpdateThrowing()
    {
        Vector3 prevPos = thrownHookModel.transform.position;
        
        // 갈고리가 목표 지점으로 날아감
        thrownHookModel.transform.position = Vector3.MoveTowards(thrownHookModel.transform.position, targetThrowPos, throwSpeed * Time.deltaTime);
        
        // (옵션) 날아가는 동안 빙글빙글 도는 애니메이션 효과
        thrownHookModel.transform.Rotate(Vector3.right * 720f * Time.deltaTime);

        // 이전 프레임 위치에서 현재 위치까지 부딪히는 물건이 있는지 선(SphereCast)을 그어 탐색
        float moveDist = Vector3.Distance(prevPos, thrownHookModel.transform.position);
        RaycastHit hit;
        if (Physics.SphereCast(prevPos, hookHitRadius, (thrownHookModel.transform.position - prevPos).normalized, out hit, moveDist, collectibleLayer))
        {
            // 물체와 충돌 완료!
            hookedObject = hit.transform;
            currentState = HookState.Returning;
            
            // 시각적 싱크를 위해 갈고리를 충돌 지점에 맞춤
            thrownHookModel.transform.position = hit.point;
            return;
        }

        // 클릭을 떼면 중간에 되돌아오게끔 처리 (혹은 최대 거리 도달 시)
        if (fireAction.WasReleasedThisFrame() || !fireAction.IsPressed() || Vector3.Distance(thrownHookModel.transform.position, targetThrowPos) < 0.1f)
        {
            currentState = HookState.Returning;
        }
    }

    private void UpdateReturning()
    {
        // 돌아올 목표 위치는 Head가 원래 달려있던 위치(총구 끝 등)
        Vector3 targetReturnPos = equipSys.handPoint.position;
        if (headParent != null)
        {
            targetReturnPos = headParent.TransformPoint(originalHeadLocalPos);
        }

        // 다시 당겨옴
        thrownHookModel.transform.position = Vector3.MoveTowards(thrownHookModel.transform.position, targetReturnPos, pullSpeed * Time.deltaTime);
        
        // 잡은 물체가 있다면 갈고리 모델을 따라 같이 옴
        if (hookedObject != null)
        {
            hookedObject.position = thrownHookModel.transform.position;
        }

        // 제자리에 도착하면(거리가 가까워지면) 회수 완료
        if (Vector3.Distance(thrownHookModel.transform.position, targetReturnPos) < 1.0f)
        {
            if (hookedObject != null)
            {
                Debris debris = hookedObject.GetComponent<Debris>();
                if (debris != null && debris.itemData != null)
                {
                    InventoryManager.Instance.AddItem(debris.itemData, 1);
                    Debug.Log($"[아이템 획득] {debris.itemData.itemName} 얻음!");
                }
                Destroy(hookedObject.gameObject);
            }

            ResetHookImmediatly();
        }
    }

    private void ResetHookImmediatly()
    {
        currentState = HookState.Idle;
        hookedObject = null;
        lineRenderer.enabled = false;

        // 갈고리 모듈(Head)을 원래 부모 및 기존 위치, 회전값으로 부착 원상복구
        if (thrownHookModel != null)
        {
            if (headParent != null)
            {
                thrownHookModel.transform.SetParent(headParent);
                thrownHookModel.transform.localPosition = originalHeadLocalPos;
                thrownHookModel.transform.localRotation = originalHeadLocalRot;
            }
            else
            {
                // 인벤토리 스위칭 등으로 손잡이(Body)가 먼저 파괴된 상태라면 분리된 Head도 마저 파괴
                Destroy(thrownHookModel);
            }
        }
        
        thrownHookModel = null;
        hookRootModel = null;
        headParent = null;
    }

    private void UpdateRope()
    {
        // 선(밧줄) 그리기 애니메이션
        if (currentState != HookState.Idle && thrownHookModel != null)
        {
            // 발사 시작 위치를 'Body' 로 잡기 (없으면 handPoint 대체)
            Vector3 startPos = equipSys.handPoint != null ? equipSys.handPoint.position : transform.position;
            if (hookRootModel != null)
            {
                Transform bodyTransform = hookRootModel.transform.Find("Body");
                if (bodyTransform != null) startPos = bodyTransform.position;
            }

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, thrownHookModel.transform.position);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}
