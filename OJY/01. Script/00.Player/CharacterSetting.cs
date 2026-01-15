using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSetting : MonoBehaviourPun
{
    //-------------------------------------------------------------------------------------
    int previousIndex_Hair = 0;
    int previousIndex_Acc = -1;
    int previousIndex_Body = 0;
    int previousIndex_Pants = 0;
    int previousIndex_Foot = 0;
    //-------------------------------------------------------------------------------------
    //의상 오브젝트를 저장할 배열들
    [SerializeField]
    GameObject[] hairItems;
    [SerializeField]
    GameObject[] bodyItems;
    [SerializeField]
    GameObject[] pantsItems;
    [SerializeField]
    GameObject[] footItems;
    [SerializeField]
    GameObject[] accItems;
    //-------------------------------------------------------------------------------------

    /// <summary>
    /// 머리색상 변경
    /// </summary>
    /// <param name="currentIndex">선택된 인덱스</param>
    public void SetHairItem(int currentIndex)
    {
        previousIndex_Hair = currentIndex;  //인덱스 저장

        //의상 전체 비활성화
        foreach (GameObject item in hairItems)
        {
            item.SetActive(false);
        }

        hairItems[currentIndex].SetActive(true); //다른 아이템 선택 시 활성화
    }

    /// <summary>
    /// 몸통 아이템 변경
    /// </summary>
    /// <param name="currentIndex">선택된 인덱스</param>
    public void SetBodyItem(int currentIndex)
    {
        if (previousIndex_Body == currentIndex) return; //이전과 같은 아이템을 선택했을때 함수 종료
        previousIndex_Body = currentIndex; //인덱스 저장

        //의상 전체 비활성화
        foreach (GameObject item in bodyItems)
        {
            item.SetActive(false);
        }

        if (currentIndex == -1) return; //선택된 아이템이 없으면 리턴

        //선택한 의상 활성화
        bodyItems[currentIndex].SetActive(true);
    }

    /// <summary>
    /// 하의 아이템 변경
    /// </summary>
    /// <param name="currentIndex"></param>
    public void SetPantsItem(int currentIndex)
    {
        if (previousIndex_Pants == currentIndex) return; //이전과 같은 아이템을 선택했을때 함수 종료
        previousIndex_Pants = currentIndex; //인덱스 저장

        //의상 전체 비활성화
        foreach (GameObject item in pantsItems)
        {
            item.SetActive(false);
        }

        if (currentIndex == -1) return; //선택된 아이템이 없으면 리턴       

        //선택한 의상 활성화
        pantsItems[currentIndex].SetActive(true);
    }

    /// <summary>
    /// 발 아이템 변경
    /// </summary>
    /// <param name="currentIndex">선택된 인덱스</param>
    public void SetFootItem(int currentIndex)
    {
        if (previousIndex_Foot == currentIndex) return; //이전과 같은 아이템을 선택했을때 함수 종료

        //의상 전체 비활성화
        foreach (GameObject item in footItems)
        {
            item.SetActive(false);
        }

        if (currentIndex == -1) return; //선택된 아이템이 없으면 리턴

        //선택한 의상 활성화
        footItems[currentIndex].SetActive(true);
        previousIndex_Foot = currentIndex; //인덱스 저장
    }

    /// <summary>
    /// 머리 아이템 변경, 같은 아이템을 선택하면 비활성화
    /// </summary>
    /// <param name="currentIndex">선택된 인덱스</param>
    public void SetAccItem(int currentIndex)
    {
        //의상 전체 비활성화
        foreach (GameObject item in accItems)
        {
            item.SetActive(false);
        }

        //같은 아이템을 다시 선택하면 비활성화
        if (previousIndex_Acc == currentIndex)
        {
            previousIndex_Acc = -1;  //선택 해제 상태로 변경
        }
        else
        {
            accItems[currentIndex].SetActive(true); //다른 아이템 선택 시 활성화
            previousIndex_Acc = currentIndex;       //인덱스 저장
        }
    }

    /// <summary>
    /// 저장된 의상 선택
    /// </summary>
    /// <param name="headIndex">머리 아이템 인덱스</param>
    /// <param name="BodyIndex">몸 아이템 인덱스</param>
    /// <param name="FootIndex">발 아이템 인덱스</param>
    public void Load_Setting(int headIndex, int BodyIndex, int PantsIndex, int FootIndex, int AccIndex)
    {
        SetHairItem(headIndex);
        SetBodyItem(BodyIndex);
        SetPantsItem(PantsIndex);
        SetFootItem(FootIndex);
        SetAccItem(AccIndex);
    }

    // 2025-02-06 처음 생성되었을때, Owner의 커스텀프로퍼티를 통해 의상 변경
    private void Start()
    {
        if (photonView != null && photonView.Owner != null)
        {
            //인게임 생성
            var hashTable = photonView.Owner.CustomProperties;
            Load_Setting((int)hashTable["Hair"], (int)hashTable["Body"], (int)hashTable["Pants"], (int)hashTable["Foot"], (int)hashTable["Accesory"]);
        }
        else
        {
            //커스터마이징
            //Load_Setting(PlayerPrefs.GetInt("Hair"), PlayerPrefs.GetInt("Body"), PlayerPrefs.GetInt("Foot"), PlayerPrefs.GetInt("Accesory"));
            if(SceneManager.GetActiveScene().name =="LobbyScene")
                Load_Setting(0, 0, 0, 0, -1);
            else
                Load_Setting(PlayerPrefs.GetInt("Hair"), PlayerPrefs.GetInt("Body"), PlayerPrefs.GetInt("Pants"), PlayerPrefs.GetInt("Foot"), PlayerPrefs.GetInt("Accesory"));
        }
    }
}
