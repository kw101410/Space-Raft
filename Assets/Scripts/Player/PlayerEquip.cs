using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquip : MonoBehaviour
{
    [Header("Equip Settings")]
    public Transform handPoint; // 플레이어 카메라 앞쪽의 아이템을 들 위치
    private GameObject currentEquippedModel; // 현재 손에 들고 있는 3D 모델
    
    public int currentHotbarIndex = 0; // 선택된 핫바 슬롯 번호 (0~8)

    private void Start()
    {
        // 손 위치가 비어있다면 카메라 자식으로 자동 생성 (유저 편의성)
        if (handPoint == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                GameObject hp = new GameObject("HandPoint");
                hp.transform.SetParent(cam.transform);
                // 화면의 우측 하단쯤으로 손 위치 잡기
                hp.transform.localPosition = new Vector3(0.5f, -0.4f, 0.7f); 
                hp.transform.localRotation = Quaternion.identity;
                handPoint = hp.transform;
            }
        }

        // 초기 무기 장착 업데이트
        EquipItem(0);
        
        // 인벤토리가 변경될 때마다 내 손에 든 아이템 개수가 다 떨어졌는지 확인하도록 구독
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += RefreshEquip;
        }
    }

    private void Update()
    {
        // 키보드 1~9번 누르면 해당 핫바 슬롯 선택
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
            {
                EquipItem(i);
            }
        }
    }

    public void EquipItem(int index)
    {
        currentHotbarIndex = index;
        RefreshEquip();

        // 디버깅 문구 (어떤 칸을 골랐는지 표시)
        Debug.Log($"[핫바] {index + 1}번 슬롯 선택됨");
    }

    // 현재 선택된 슬롯의 아이템을 꺼내서 모델로 보여주거나 파괴
    private void RefreshEquip()
    {
        if (InventoryManager.Instance == null) return;

        // 인벤토리 매니저의 slots 중 0~8번이 핫바로 사용됨
        InventorySlot selectedSlot = InventoryManager.Instance.slots[currentHotbarIndex];

        // 손에 든 모델 지우기
        if (currentEquippedModel != null)
        {
            Destroy(currentEquippedModel);
        }

        // 해당 슬롯에 아이템이 있고, 그 아이템에 '손에 들 프리팹(모델)'이 지정되어 있다면 생성
        if (selectedSlot.item != null && selectedSlot.item.prefab != null && selectedSlot.count > 0)
        {
            currentEquippedModel = Instantiate(selectedSlot.item.prefab, handPoint);
            currentEquippedModel.transform.localPosition = Vector3.zero;
            currentEquippedModel.transform.localRotation = Quaternion.identity;
            
            // 만약 물리엔진(Rigidbody)이나 기존 기능 찌꺼기가 남은 거라면 무기 상태로 꺼주기 (선택)
            Rigidbody rb = currentEquippedModel.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
        }
    }
}
