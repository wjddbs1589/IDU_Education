using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSpawner : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    [Header("Character Prefab")]
    [SerializeField] GameObject Prefab_Chanhee;
    [SerializeField] GameObject Prefab_Gayeong;
    [SerializeField] GameObject Prefab_Haejin;
    [SerializeField] GameObject Prefab_Hana;

    //-------------------------------------------------------------------------------------
    GameObject spawnedCharacter;   //생성된 캐릭터
    GameObject SpawnPos;           //생성 위치에 있는 오브젝트
    float spawnPosRot = -12.5f;    //시작시 지정 되는 값
    float currentRot;              //회전의 기준이 되는 값
    [SerializeField] Button Btn_Rotate;
    bool bRot;
    float distance;
    Vector2 startPosition;    
    //-------------------------------------------------------------------------------------
    private void Awake()
    {
        SpawnPos = transform.GetChild(0).gameObject;
        //SpawnCharacter(0);

        //이벤트 추가
        EventTrigger trigger_L = Btn_Rotate.gameObject.AddComponent<EventTrigger>();

        //누를 때 
        EventTrigger.Entry entryDown_L = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };        
        entryDown_L.callback.AddListener((data) => Rotate_Down());
        trigger_L.triggers.Add(entryDown_L);

        //뗄 때 
        EventTrigger.Entry entryUp_L = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp_L.callback.AddListener((data) => Rotate_Up());
        trigger_L.triggers.Add(entryUp_L);

        //초기 회전값 세팅
        Quaternion rot = Quaternion.Euler(0, spawnPosRot, 0);
        SpawnPos.transform.rotation = rot;
    }

    private void Update()
    {
        if (bRot)
        {            
            distance = startPosition.x - Camera.main.ScreenToViewportPoint(Input.mousePosition).x;            
            Quaternion rot = Quaternion.Euler(0, currentRot + (distance * 360), 0);
            SpawnPos.transform.rotation = rot;
        }
    }

    /// <summary>
    /// 인덱스에 해당하는 캐릭터 생성
    /// </summary>
    /// <param name="index"></param>
    public void SpawnCharacter(int index)
    {
        DestroyModel(); //기존 모델 삭제

        //인덱스에 따라 캐릭터 생성
        switch (index)
        {
            case 0:
                spawnedCharacter = Instantiate(Prefab_Chanhee, SpawnPos.transform);
                break;
            case 1:
                spawnedCharacter = Instantiate(Prefab_Gayeong, SpawnPos.transform);
                break;
            case 2:
                spawnedCharacter = Instantiate(Prefab_Haejin, SpawnPos.transform);
                break;
            case 3:
                spawnedCharacter = Instantiate(Prefab_Hana, SpawnPos.transform);
                break;
            default:
                break;
        }

        //지정된 위치로 캐릭터 배치
        spawnedCharacter.transform.position = SpawnPos.transform.position;
        spawnedCharacter.transform.rotation = Quaternion.Euler(0, spawnPosRot, 0);
    }

    /// <summary>
    /// 기존에 존재하는 캐릭터 모델링이 있으면 삭제
    /// </summary>
    void DestroyModel()
    {
        if (!spawnedCharacter) return;

        Destroy(spawnedCharacter);
    }

    /// <summary>
    /// 생성된 캐릭터의 정보 반환
    /// </summary>
    /// <returns>의상정보를 가지고 있는 클래스</returns>
    public CharacterSetting GetCharacterInfo()
    {
        if (!spawnedCharacter) return null;

        return spawnedCharacter.GetComponent<CharacterSetting>();
    }

    void Rotate_Down()
    {
        bRot = true;
        startPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition); //클릭이 시작된 좌표 수집
        currentRot = SpawnPos.transform.eulerAngles.y;                          //기준이되는 회전값 수집
    }
    void Rotate_Up()
    {
        bRot = false;
    }
}
