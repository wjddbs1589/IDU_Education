using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClothCanvas : MonoBehaviour
{
    [Header("Btn_Category")]
    [SerializeField]
    Button[] btns_CategorySelect;
    //-------------------------------------------------------------------------------------
    [Header("Btn_End")]
    [SerializeField] Button btn_Done;

    [Header("Btn_Exit")]
    [SerializeField] Button btn_Exit;
    //-------------------------------------------------------------------------------------
    [SerializeField]
    GameObject ItemBtnPrefab; //생성될 버튼 프리펩
    int AccessoriesCount;     //캐릭터의 액세서리 갯수
    public bool isMale = true;

    Button TempItemBtn; //생성된 버튼의 바인딩을 위한 임시버튼

    [Header("생성될 버튼이 저장될 리스트")]
    public List<GameObject> ItemBtnList = new List<GameObject>(3);

    [Header("버튼을 배치할 부모")]
    [SerializeField] Transform ItemDisplayObj;

    int Index_Category = 0;       //카테고리 인덱스
    int BtnIndex = 0;             //현재 선택한 버튼 인덱스
    int Index_Hair = 0;           //의상 인덱스
    int Index_Body = 0;           //의상 인덱스
    int Index_Pants = 0;          //의상 인덱스
    int Index_Foot = 0;           //신발 인덱스
    int Index_Accessories = -1;   //모자 인덱스
    int Index_Accessories_ex = -1;//이전 모자 인덱스
    //-------------------------------------------------------------------------------------
    [Header("캐릭터 변경 버튼")]
    [SerializeField] Button btn_ChangeCharacter; //캐릭터 변경 버튼

    int Index_Character; //현재 선택된 캐릭터의 인덱스

    CharacterSetting characterSetting; //캐릭터의 의상 정보를 가지고 있는 클래스
    CharacterSpawner characterSpawner; //캐릭터를 생성하는 클래스(캐릭터 모델 삭제 및 생성)

    [SerializeField] TextMeshProUGUI CharNameText; //캐릭터 이름
    //-------------------------------------------------------------------------------------
    [Header("로비매니저")]
    [SerializeField] GameObject lobbyManagerObj;

    #region 초기설정    
    private void OnEnable()
    {
        Init();
    }

    private void Awake()
    {
        characterSpawner = FindObjectOfType<CharacterSpawner>(); //스포너 정보 수집
        ButtonSetting(); //버튼 바인딩
    }

    /// <summary>
    /// 초기세팅
    /// </summary>
    void Init()
    {
        //선택한 성별에 따른 캐릭터 생성
        if (isMale)
        {
            Index_Character = 0;
            characterSpawner.SpawnCharacter(0);
            Set_CharacterName(0);
        }
        else
        {
            Index_Character = 1;
            characterSpawner.SpawnCharacter(1);
            Set_CharacterName(1);
        }

        characterSetting = characterSpawner.GetCharacterInfo();

        Reset_Customize();  //초기화
        Btn_HairCategory(); //헤어 버튼 생성
    }
    #endregion
    //-------------------------------------------------------------------------------------     
    /// <summary>
    /// 버튼 바인딩
    /// </summary>
    void ButtonSetting()
    {
        //----------카테고리 선택 버튼----------
        btns_CategorySelect[0].onClick.AddListener(Btn_HairCategory);
        btns_CategorySelect[1].onClick.AddListener(Btn_BodyCategory);
        btns_CategorySelect[2].onClick.AddListener(Btn_PantsCategory);
        btns_CategorySelect[3].onClick.AddListener(Btn_FootCategory);
        btns_CategorySelect[4].onClick.AddListener(Btn_AccessoriesCategory);

        //----------종료 버튼----------
        btn_Done.onClick.AddListener(Click_Btn_Done);
        btn_Exit.onClick.AddListener(delegate { lobbyManagerObj.SetActive(true); transform.parent.gameObject.SetActive(false); });

        //----------캐릭터 변경 버튼----------
        btn_ChangeCharacter.onClick.AddListener(Btn_CharacterChange);
    }

    #region 의상 카테고리 변경
    /// <summary>
    /// 머리 버튼을 눌렀을 때
    /// </summary>
    void Btn_HairCategory()
    {
        //카테고리 UI변경      
        Index_Category = 0;
        Set_Category_Image(Index_Category);

        DestroyButtons();
        SpawnClothBtn(ItemDisplayObj, 0, 3);
        SetUI_SelectedItem(ItemBtnList, Index_Hair);
    }

    /// <summary>
    /// 몸통 버튼을 눌렀을 때
    /// </summary>
    void Btn_BodyCategory()
    {
        //카테고리 UI변경
        Index_Category = 1;
        Set_Category_Image(Index_Category);

        DestroyButtons();
        SpawnClothBtn(ItemDisplayObj, 1, 3);  //리스트 1열에 상의버튼 3개 생성
        SetUI_SelectedItem(ItemBtnList, Index_Body);
    }

    /// <summary>
    /// 몸통 버튼을 눌렀을 때
    /// </summary>
    void Btn_PantsCategory()
    {
        //카테고리 UI변경
        Index_Category = 2;
        Set_Category_Image(Index_Category);

        DestroyButtons();
        if (Index_Character == 3) //'하나'는 바지가 2개
        {
            SpawnClothBtn(ItemDisplayObj, 2, 2); //리스트 2열에 하의버튼 3개 생성
        }
        else
        {
            SpawnClothBtn(ItemDisplayObj, 2, 3);  //리스트 1열에 상의버튼 3개 생성
        }
        SetUI_SelectedItem(ItemBtnList, Index_Pants);
    }

    /// <summary>
    /// 발 버튼을 눌렀을 때
    /// </summary>
    void Btn_FootCategory()
    {
        //카테고리 UI변경
        Index_Category = 3;
        Set_Category_Image(Index_Category);

        DestroyButtons();       
        SpawnClothBtn(ItemDisplayObj, 3, 3); //리스트 2열에 하의버튼 3개 생성
        SetUI_SelectedItem(ItemBtnList, Index_Foot);
    }

    /// <summary>
    /// 액세서리 버튼을 눌렀을 때
    /// </summary>
    void Btn_AccessoriesCategory()
    {
        //카테고리 UI변경
        Index_Category = 4;
        Set_Category_Image(Index_Category);

        if (isMale)
        {
            AccessoriesCount = 0;
        }
        else
        {
            if (Index_Character == 1)
            {
                AccessoriesCount = 3;
            }
            else if (Index_Character == 3)
            {
                AccessoriesCount = 2;
            }
        }

        DestroyButtons();
        SpawnClothBtn(ItemDisplayObj, 4, AccessoriesCount); //리스트 3열에 악세버튼 AccessoriesCount개 생성
        SetUI_SelectedItem(ItemBtnList, Index_Accessories);
    }
    #endregion

    #region 의상 선택 
    /// <summary>
    /// 의상 선택 버튼 생성
    /// </summary>
    /// <param name="count">생성할 버튼 갯수</param>
    /// <param name="clothCategoryIndex">생성할 의상 카테고리</param>
    /// <param name="parent">버튼의 부모가 되는 트랜스폼</param>
    void SpawnClothBtn(Transform parent, int clothCategoryIndex, int count)
    {
        GameObject tempObj; //임시 오브젝트

        for (int i = 0; i < count; i++)
        {
            //버튼생성 
            tempObj = Instantiate(ItemBtnPrefab);

            ItemBtnList.Add(tempObj);

            //버튼 바인딩
            int index = i;
            TempItemBtn = tempObj.GetComponentInChildren<Button>();
            TempItemBtn.onClick.AddListener(delegate { Click_Btn_Item(clothCategoryIndex, index); });

            //부모 스크롤뷰 설정
            tempObj.transform.SetParent(parent, false); //false를 붙여야 동적 생성되는 UI가 Canvas Scaler의 영향을 받는다.

            //버튼의 스프라이트 변경
            BtnClothSprite BtnSet = TempItemBtn.GetComponent<BtnClothSprite>();
            BtnSet.SetBtnSprite(clothCategoryIndex, Index_Character, index);
        }
    }

    /// <summary>
    /// 리스트에 존재하는 버튼 삭제
    /// </summary>
    void DestroyButtons()
    {
        if (ItemBtnList.Count > 0)
        {
            foreach (GameObject t in ItemBtnList)
            {
                Destroy(t);
            }

            ItemBtnList.Clear();
        }
    }

    /// <summary>
    /// 현재 선택된 카테고리에 따른 상호작용
    /// </summary>
    /// <param name="index"></param>
    void Click_Btn_Item(int categoryIndex, int index)
    {
        if (!characterSetting) return;
        SetUI_SelectedItem(ItemBtnList, index);

        //의상 변경
        if (categoryIndex == 0) //헤어
        {
            characterSetting.SetHairItem(index);

            //값 저장
            Index_Hair = index;
        }
        else if (categoryIndex == 1) //상의
        {
            characterSetting.SetBodyItem(index);

            //캐릭터가 '하나'일 때
            if (Index_Character == 3)
            {
                //원피스를 선택했을 때
                if (index == 2) 
                {
                    //바지 비 활성화
                    Index_Pants = -1;
                    characterSetting.SetPantsItem(Index_Pants);
                }
                //원피스에서 다른 상의로 변경될 때
                //0번상의 강제 선택
                else if(Index_Body == 2)
                {
                    Index_Pants = 0;
                    characterSetting.SetPantsItem(Index_Pants);
                }
            }

            //값 저장
            Index_Body = index;
        }
        else if (categoryIndex == 2) //하의
        {
            characterSetting.SetPantsItem(index);

            //'하나'가 원피스를 입고 있을때
            //바지를 선택하면 0번 상의로 강제선택
            if (Index_Character == 3 && Index_Body == 2)
            {   
                Index_Body = 0;
                characterSetting.SetBodyItem(Index_Body);
            }

            //값 저장
            Index_Pants = index;
        }
        else if (categoryIndex == 3) //신발
        {
            characterSetting.SetFootItem(index);

            //값 저장
            Index_Foot = index; 
        }
        else if (categoryIndex == 4) //액세서리
        {
            characterSetting.SetAccItem(index);

            //현재 선택한 것이 이전과 같은것인지 확인
            //같으면 비활성화 및 값 초기화
            if (Index_Accessories_ex == index)
            {                
                Index_Accessories = -1;
                Index_Accessories_ex = -1;
                OffItemUI();
            }
            else
            {
                Index_Accessories = index;
                Index_Accessories_ex = index;
            }
        }

        // 2025-03-18 RJH 효과음 재생
        GameManager.Instance.SFXPlay(EFFECT.Customizing);
    }

    /// <summary>
    /// 완료 버튼을 눌렀을 때
    /// </summary>
    public void Click_Btn_Done()
    {
        // 2025-02-06 RJH 커스텀프로퍼티에 인덱스 저장
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Character", Index_Character }, { "Accesory", Index_Accessories }, { "Body", Index_Body }, { "Pants", Index_Pants }, { "Foot", Index_Foot }, { "Hair", Index_Hair} });
        GameManager.Instance.characterGender = (Index_Character == 0 || Index_Character == 2) ? CHARACTERGENDER.MALE : CHARACTERGENDER.FEMALE;

        // 2025-03-10 RJH 플레이어 프리팹에 저장
        PlayerPrefs.SetInt("Character", Index_Character); //캐릭터
        PlayerPrefs.SetInt("Accesory", Index_Accessories);//악세서리
        PlayerPrefs.SetInt("Hair", Index_Hair);           //헤어
        PlayerPrefs.SetInt("Body", Index_Body);           //상의
        PlayerPrefs.SetInt("Pants", Index_Pants);         //하의
        PlayerPrefs.SetInt("Foot", Index_Foot);           //신발
        PlayerPrefs.SetInt("Gender", (int)GameManager.Instance.characterGender);
        PlayerPrefs.Save();

        // 2025-03-18 RJH 효과음 재생
        GameManager.Instance.SFXPlay(EFFECT.MenuConfirm);

        // 2025-03-6 RJH 씬이동 
        PhotonManager.Instance.MoveScene(SCENARIOPLACE.PLAZA, false);
    }
    #endregion
    //-------------------------------------------------------------------------------------
    #region 캐릭터 변경    
    /// <summary>
    /// 이전 캐릭터로 변경
    /// </summary>
    void Btn_CharacterChange()
    {
        if (isMale) Index_Character = (Index_Character == 0) ? 2 : 0;
        else Index_Character = (Index_Character == 1) ? 3 : 1;

        Set_CharacterName(Index_Character);
        characterSpawner.SpawnCharacter(Index_Character);       //캐릭터 생성
        characterSetting = characterSpawner.GetCharacterInfo(); //캐릭터 의상 정보 수집
        Reset_Customize();                                      //UI 리셋

        // 2025-03-18 RJH 효과음 재생
        GameManager.Instance.SFXPlay(EFFECT.CharacterChange);
    }


    /// <summary>
    /// 캐릭터의 이름 표기 변경
    /// </summary>
    /// <param name="index"></param>
    void Set_CharacterName(int index)
    {
        if (index == 0)
        {
            CharNameText.text = "찬희";
        }
        else if (index == 1)
        {
            CharNameText.text = "가영";
        }
        else if (index == 2)
        {
            CharNameText.text = "해진";
        }
        else if (index == 3)
        {
            CharNameText.text = "하나";
        }
    }

    /// <summary>
    /// 캐릭터 변경시 카테고리 및 인덱스 변경사항 리셋
    /// </summary>
    void Reset_Customize()
    {
        BtnIndex = 0;  //항목 인덱스 리셋

        //2025-02-07 RJH 선택한 의상정보도 초기화
        Index_Hair = 0;
        Index_Body = 0;
        Index_Pants = 0;
        Index_Foot = 0;
        Index_Accessories = -1;

        Btn_HairCategory(); //선택된 의상 리셋
    }
    #endregion
    //-------------------------------------------------------------------------------------

    CategoryBtnController categoryBtn; //버튼의 이미지 변경을 담당하는 클래스

    /// <summary>
    /// 선택된 카테고리의 UI 변경
    /// </summary>
    /// <param name="index"></param>
    void Set_Category_Image(int index)
    {
        foreach (Button btn in btns_CategorySelect) //배열에서 버튼정보를 수집하여 전체를 기본 이미지로 세팅
        {
            categoryBtn = btn.GetComponent<CategoryBtnController>();

            if (categoryBtn)
            {
                categoryBtn.Set_NormalImage();
            }
        }

        //해당되는 인덱스의 카테고리 이미지만 변경
        categoryBtn = btns_CategorySelect[index].GetComponent<CategoryBtnController>();
        if (categoryBtn) categoryBtn.Set_ClickedImage();
    }

    ItemBtnController itemBtn;

    /// <summary>
    /// 아이템 선백 버튼 이미지 변경
    /// </summary>
    /// <param name="index"></param>
    void SetUI_SelectedItem(List<GameObject> list, int index)
    {
        int num = 0;

        foreach (GameObject obj in list) //배열에서 버튼정보를 수집하여 전체를 기본 이미지로 세팅
        {
            itemBtn = obj.GetComponent<ItemBtnController>();
            if (!itemBtn) return;

            if (num != index)
            {
                itemBtn.NormalSetting();
            }
            else
            {
                itemBtn.ClickedSetting(); //해당되는 인덱스의 아이템 이미지만 변경
            }

            num += 1;
        }
    }

    /// <summary>
    /// 액세서리를 다시 골랐을때 ui 끄기
    /// </summary>
    void OffItemUI()
    {
        foreach (GameObject obj in ItemBtnList) //배열에서 버튼정보를 수집하여 전체를 기본 이미지로 세팅
        {
            itemBtn = obj.GetComponent<ItemBtnController>();
            if (!itemBtn) return;

            itemBtn.NormalSetting();
        }
    }
    //-------------------------------------------------------------------------------------    
}