using UnityEngine;
using BackEnd;
using UnityEngine.UI;
using TMPro;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// 뒤끝의 게임정보 데이터
/// </summary>
public class UserData
{
    public bool ActiveCheck;     //활성화 여부
    public string BackupCode;    //백업코드
    public string LicenseNumber; //라이선스 번호
    public string SchoolName;    //기관명
    public string Indate;        //등록일자
}

public class BackendGameData
{
    //-------------------------------------------------------
    public static UserData userData;
    public LitJson.JsonData LicenseTable; //서버에서 받은 전체 라이선스 정보
    private string gameDataRowInDate = string.Empty;
    LicenseManager licenseManager;
    //-------------------------------------------------------
    private static BackendGameData _instance = null;   
    public static BackendGameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendGameData();
            }

            return _instance;
        }
    }
    //-------------------------------------------------------    

    /// <summary>
    /// 입력받은 라이선스 숫자가 유효한지, 이미 사용된것이 아닌지 확인하여 반환
    /// </summary>
    /// <param name="inputNumber">사용할 라이선스</param>
    /// <returns>false = 현재 이미 사용중인 라이선스, true = 새로이 등록 가능한 라이선스</returns>
    public bool LicenseCheck(string inputNumber)
    {
        foreach (LitJson.JsonData data in LicenseTable)
        {
            //입력한 라이센스와 같은 번호가 있고 사용중이 아닌 라이선스일때 true 반환
            if ((inputNumber == data["LicenseNumber"].ToString()) && !bool.Parse(data["ActiveCheck"].ToString()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 라이선스의 정보 초기화
    /// </summary>
    public void DeleteRowData()
    {
        //업데이트할 정보
        Param param = new Param();
        param.Add("ActiveCheck", false);  //비활성화
        param.Add("SchoolName", "");      //학교명 초기화
        param.Add("RoomCode","");         //입장코드 초기화

        //특정 row에 업로드할 데이터 설정
        BackendReturnObject bro = Backend.GameData.UpdateV2
            (
                "LicenseTable",     //업데이트할 데이터 테이블
                userData.Indate,    //데이터 테이블 row의 inDate값
                Backend.UserInDate, //유저 정보. 전체 동일
                param               //업데이트할 데이터
            );
    }

    /// <summary>
    /// 입력한 라이선스의 기본 데이터 가져오기
    /// </summary>
    public void GetUserData(string license)
    {
        userData = new UserData();

        foreach (LitJson.JsonData data in LicenseTable)
        {
            if (license == data["LicenseNumber"].ToString())
            {
                userData.ActiveCheck    = bool.Parse(data["ActiveCheck"].ToString()); //활성화 여부                
                userData.LicenseNumber  = data["LicenseNumber"].ToString();           //라이선스 번호  
                userData.Indate         = data["inDate"].ToString();                  //등록일
                userData.SchoolName     = data["SchoolName"].ToString();              //학교명                
                return;
            }
        }
    }    

    /// <summary>
    /// 라이선스 등록시 정보를 서버에 저장
    /// </summary>
    public void UserDataUpdate(string schoolName)
    {
        if (userData == null) return;

        //업데이트할 데이터
        Param param = new Param();       
        param.Add("ActiveCheck", true);            //활성화 여부 변경   
        param.Add("BackupCode", SetBackupCode());  //생성된 백업코드 부여
        param.Add("SchoolName", schoolName);       //입력한 학교명 등록

        //특정 row에 업로드할 데이터 설정
        BackendReturnObject bro = Backend.GameData.UpdateV2
            (
                "LicenseTable",     //업데이트할 데이터 테이블
                userData.Indate,    //데이터 테이블 row의 inDate값
                Backend.UserInDate, //유저 정보. 전체 동일
                param               //업데이트할 데이터
            );
    }

    /// <summary>
    /// 룸 코드 업로드
    /// </summary>
    /// <param name="roomCode"></param>
    public string UploadRoomCode()
    {
        //코드 발급       
        string str = string.Empty;
        do
        {
            string temp = DateTime.Now.Ticks.ToString() + UnityEngine.Random.Range(1000, 9999); //시간 + 랜덤 값
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(temp));
                str = BitConverter.ToString(hash).Replace("-", "").Substring(0, 6); //6자리 해시 값 반환
            }
        }        
        //반환값이 false(곂치는 항목 있음)이면 반복실행, true(곂치는 항목 없음)일때 탈출
        while (!EntranceCodeCheck(str));       

        PlayerPrefs.DeleteKey("Character");
        PlayerPrefs.SetString("RoomCode", str); //기기에 룸코드 저장

        //업데이트할 데이터
        Param param = new Param();
        param.Add("RoomCode", str);   

        //특정 row에 업로드할 데이터 설정
        BackendReturnObject bro = Backend.GameData.UpdateV2
            (
                "LicenseTable",     //업데이트할 데이터 테이블
                userData.Indate,    //데이터 테이블 row의 inDate값
                Backend.UserInDate, //유저 정보. 전체 동일
                param               //업데이트할 데이터
            );

        return str;
    }

    /// <summary>
    /// 입력받은 코드의 중복 확인
    /// </summary>
    /// <param name="code">생성된 코드</param>
    /// <returns>false = 중복 O, true = 중복 X</returns>
    bool EntranceCodeCheck(string str)
    {
        foreach (LitJson.JsonData data in LicenseTable)
        {
            //Debug.Log($"생성된 코드 : {str} / 존재하던 코드 : {data["RoomCode"].ToString()}");

            //겹치는 코드가 있으면 false(사용불가)반환
            if (str == data["RoomCode"].ToString())
            {    
                return false;
            }
        }
        //겹치는 항목이 없으면 true반환
        return true;    
    }

    /// <summary>
    /// 백업코드 생성 및 반환
    /// </summary>
    /// <returns>생성된 백업코드</returns>
    string SetBackupCode()
    {
        //백업코드 생성
        string backupCode = DateTime.Now.Ticks.ToString() + UnityEngine.Random.Range(1000, 9999); //시간 + 랜덤 값
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(backupCode));
            userData.BackupCode = BitConverter.ToString(hash).Replace("-", "").Substring(0, 6); //6자리 해시 값 반환
        }

        return userData.BackupCode;
    }

    /// <summary>
    /// 백업코드 입력받아 라이선스 반환
    /// </summary>
    /// <param name="backupCode"></param>
    /// <returns></returns>
    public string GetLicenseNumber(string backupCode)
    {
        userData = new UserData();
                
        if(LicenseTable.Count > 0) 
        {
            foreach (LitJson.JsonData data in LicenseTable)
            {
                if (null != data["BackupCode"].ToString())
                {
                    Debug.Log(data["BackupCode"].ToString());
                }
            }

            foreach (LitJson.JsonData data in LicenseTable)
            {
                if (backupCode == data["BackupCode"].ToString())
                {
                    return data["LicenseNumber"].ToString();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 유저의 정보 데이터 반환
    /// </summary>
    /// <returns>유저 데이터</returns>
    public UserData GetRowData() 
    {
        return userData;
    }

    /// <summary>
    /// 저장된 정보가 유효한 정보인지 확인하여 반환
    /// </summary>
    /// <returns>true = 등록된 기기, false = 등록되지 않은 기기</returns>
    public bool RegistryDataCheck()
    {
        foreach (LitJson.JsonData data in LicenseTable)
        {
            //서버에 저장된 정보와 일치한지 확인
            if ((PlayerPrefs.GetInt("bActivate", 0) == 1) == bool.Parse(data["ActiveCheck"].ToString()) &&
                PlayerPrefs.GetString("License") == data["LicenseNumber"].ToString() &&
                PlayerPrefs.GetString("SchoolName") == data["SchoolName"].ToString() &&
                PlayerPrefs.GetString("inDate") == data["inDate"].ToString())
            {
                GetUserData(PlayerPrefs.GetString("License")); //저장된 정보 불러오기                
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 메인 클래스
/// </summary>
public class LicenseManager : MonoBehaviour
{
    #region 변수    
    BackendGameData GameData;   //게임정보 관리 클래스
    UserData userData;          //게임정보 구조체
    //-------------------------------------------------------    
    [SerializeField]
    GameObject LicenseRegister;

    [SerializeField]
    GameObject roomCodeMaker;
    //-------------------------------------------------------
    [Header("직업선택 창")]
    [SerializeField]
    GameObject obj_job;

    [Header("Extra Btn.zip")]
    [SerializeField]
    GameObject Buttons;

    Button[] ExtraBtns;
    //-------------------------------------------------------
    [Header("00-01.라이선스 입력")]
    [SerializeField]
    GameObject obj_licenseUI;     //라이선스 UI

    [SerializeField]
    GameObject Popup_L;           //안내 팝업

    TMP_InputField Input_License; //라이선스 인풋필드
    Button Btn_CheckL;            //라이선스 제출 버튼
    Button Btn_ClosePopup_L;      //팝업 닫기 버튼

    string licenseNumber;         //입력한 라이선스 숫자
    //-------------------------------------------------------
    [Header("01-01.라이선스 찾기")]
    [SerializeField]
    GameObject Popup_FindLicense;          //라이선스 찾기 팝업

    TMP_InputField Input_BackupCode;       //백업 코드를 입력
    Button Btn_CheckBackupCode;            //백업코드 제출 버튼
    Button Btn_ClosePopup_InputBackupCode; //백업코드 입력창 닫기 버튼

    [Header("01-02.라이선스 찾기 결과")]
    [SerializeField]
    GameObject Popup_FindResult;           //결과창 팝업

    TextMeshProUGUI Text_FindResult;       //결과 안내 텍스트
    Button Btn_ClosePopup_FindReset;       //결과 안내창 닫기 버튼
    //-------------------------------------------------------
    [Header("02-01.등록된 라이선스 비활성화")]
    [SerializeField]
    GameObject Popup_CancelLicense;     //팝업

    TMP_InputField Input_CancelLicense; //인풋 = 라이선스
    Button Btn_StartReset;              //시작 버튼
    Button Btn_CloseReset;              //닫기 버튼

    [Header("02-02. 라이선스 비활성화 결과")]
    [SerializeField]
    GameObject Popup_CancelResult;      //리셋 결과 창

    TextMeshProUGUI Text_CancelResult;  //결과 안내 텍스트
    Button Btn_ClosePopup_Cancel;       //팝업 닫기 버튼

    //-------------------------------------------------------    
    [Header("03-01.학교명 입력")]
    [SerializeField]
    GameObject obj_schoolNameUI;       //학교명 UI

    TMP_InputField Input_SchoolName;   //학교명 인풋필드
    Button Btn_CheckSchoolName;        //학교명 제출 버튼

    [SerializeField]
    GameObject Popup_BadName;          //안내 팝업
    Button Btn_ClosePopup_SchoolName;  //팝업 닫기 버튼
    string schoolName;                 //입력한 학교명
    Button Btn_Back;                   //이전 창으로 돌아가기 버튼 
    //-------------------------------------------------------  
    [Header("03-02.학교명 등록 확인")]
    [SerializeField] 
    GameObject Popup_Conrim;           //안내 팝업창

    TextMeshProUGUI Text_Confirm;      //학교명 인풋필드
    Button Btn_ClosePopup_NameOk;      //학교명 등록 완료 버튼
    Button Btn_ClosePopup_NameNo;      //학교명 등록 취소 버튼
    //-------------------------------------------------------  
    [Header("03-03.백업코드 안내")]
    [SerializeField] 
    GameObject Popup_End;              //안내 팝업창

    TextMeshProUGUI Text_End;          //안내 텍스트
    Button Btn_PopupEnd;               //닫기 버튼
    //-------------------------------------------------------  
    [Header("뒤끝 연결 팝업")]
    public GameObject Popup_Connecting;       //안내 팝업창    
    //-------------------------------------------------------  
    List<Dictionary<string, object>> slang = new List<Dictionary<string, object>>();
    #endregion
    //-------------------------------------------------------  
    [Header("업데이트 팝업")]
    [SerializeField] GameObject Popup_Update;
    [SerializeField] Button Btn_ExitProgram;
    private void Awake()
    {
        Init();
        ServerConnect();
    }

    /// <summary>
    /// 초기 컴포넌트 정보 수집
    /// </summary>
    void Init()
    {
        //찾기, 초기화, 나가기 버튼
        ExtraBtns = Buttons.GetComponentsInChildren<Button>();

        //라이선스 입력
        Input_License = obj_licenseUI.GetComponentInChildren<TMP_InputField>();
        Btn_CheckL = obj_licenseUI.GetComponentInChildren<Button>();
        Btn_ClosePopup_L = Popup_L.GetComponentInChildren<Button>();

        //라이선스 찾기
        Input_BackupCode = Popup_FindLicense.GetComponentInChildren<TMP_InputField>();

        Button[] btnF = new Button[2];
        btnF = Popup_FindLicense.GetComponentsInChildren<Button>();
        Btn_CheckBackupCode = btnF[0];
        Btn_ClosePopup_InputBackupCode = btnF[1];

        //라이선스 찾기 결과
        Text_FindResult = Popup_FindResult.GetComponentInChildren<TextMeshProUGUI>();
        Btn_ClosePopup_FindReset = Popup_FindResult.GetComponentInChildren<Button>();

        //라이선스 비활성화
        Input_CancelLicense = Popup_CancelLicense.GetComponentInChildren<TMP_InputField>();

        Button[] btnR = new Button[2];
        btnR = Popup_CancelLicense.GetComponentsInChildren<Button>();
        Btn_StartReset = btnR[0];
        Btn_CloseReset = btnR[1];

        //라이선스 비활성화 결과
        Text_CancelResult = Popup_CancelResult.GetComponentInChildren<TextMeshProUGUI>();
        Btn_ClosePopup_Cancel = Popup_CancelResult.GetComponentInChildren<Button>();

        //학교명 입력
        Input_SchoolName = obj_schoolNameUI.GetComponentInChildren<TMP_InputField>();
        Btn_CheckSchoolName = obj_schoolNameUI.GetComponentInChildren<Button>();
        Btn_ClosePopup_SchoolName = Popup_BadName.GetComponentInChildren<Button>();

        Button[] btnS = new Button[2];
        btnS = obj_schoolNameUI.GetComponentsInChildren<Button>();
        Btn_Back = btnS[1];

        //학교명 등록 확인
        Text_Confirm = Popup_Conrim.GetComponentInChildren<TextMeshProUGUI>();
        Button[] btnC = new Button[2];
        btnC = Popup_Conrim.GetComponentsInChildren<Button>();
        Btn_ClosePopup_NameOk = btnC[0];
        Btn_ClosePopup_NameNo = btnC[1];

        //백업코드 안내
        Text_End = Popup_End.GetComponentInChildren<TextMeshProUGUI>();
        Btn_PopupEnd = Popup_End.GetComponentInChildren<Button>();

        ButtonBinding();

        GameData = BackendGameData.Instance; //테이블 데이터 인스턴스 저장
        slang = CSVReader.Read("slang");     //필터링 단어 목록
    }

    /// <summary>
    /// 버튼 오브젝트 바인딩
    /// </summary>
    void ButtonBinding()
    {
        //-----라이선스 입력-----
        Btn_CheckL.onClick.AddListener(CheckLicense);                                //라이선스 제출 버튼
        Btn_ClosePopup_L.onClick.AddListener(delegate { Popup_Close(Popup_L); });  //라이선스 오류 팝업 닫기 버튼


        //-----라이선스 찾기-----         
        ExtraBtns[0].onClick.AddListener(
            delegate
            {
                Input_BackupCode.text = "";     //인풋필드 초기화
                Popup_Open(Popup_FindLicense);  //라이선스 찾기 버튼
            });

        Btn_CheckBackupCode.onClick.AddListener(delegate { Check_backupCode(); });                     //백업코드 제출 버튼
        Btn_ClosePopup_InputBackupCode.onClick.AddListener(delegate { Popup_Close(Popup_FindLicense); }); //라이선스 찾기 취소 버튼
        Btn_ClosePopup_FindReset.onClick.AddListener(delegate { Popup_Close(Popup_FindResult); });        //탐색 결과 닫기 버튼

        //-----라이선스 비활성화-----
        ExtraBtns[1].onClick.AddListener(
            delegate
            {
                Input_CancelLicense.text = "";   //인풋필드 초기화
                Popup_Open(Popup_CancelLicense); //라이선스 비활성화창 열기 
            });

        Btn_CloseReset.onClick.AddListener(delegate { Popup_Close(Popup_CancelLicense); });       //팝업 닫기
        Btn_StartReset.onClick.AddListener(Check_ResetCode);                                      //비활성화 시작
        Btn_ClosePopup_Cancel.onClick.AddListener(delegate { Popup_Close(Popup_CancelResult); }); //라이선스 비활성화 결과창 닫기

        //-----직업선택 창으로 이동-----
        ExtraBtns[2].onClick.AddListener(BackToJobPage);

        //-----학교명 입력-----
        Btn_Back.onClick.AddListener(OpenInputField_LicenseNumber);                               //라이선스 입력 창으로 이동
        Btn_CheckSchoolName.onClick.AddListener(CheckSchoolName);                                 //학교명 제출 버튼
        Btn_ClosePopup_SchoolName.onClick.AddListener(delegate { Popup_Close(Popup_BadName); }); //학교명 오류 팝업 닫기 버튼

        //-----학교명 등록-----
        Btn_ClosePopup_NameOk.onClick.AddListener(Btn_NameOk);                              //학교명 등록 완료 버튼
        Btn_ClosePopup_NameNo.onClick.AddListener(delegate { Popup_Close(Popup_Conrim); }); //학교명 등록 취소 버튼
        Btn_PopupEnd.onClick.AddListener(                                                   //백업코드 안내 팝업 닫기
            delegate 
            { 
                Popup_Close(Popup_End);
                LicenseRegister.SetActive(false);
                roomCodeMaker.SetActive(true);
            });

        //-----업데이트 필요-----
        Btn_ExitProgram.onClick.AddListener(() => Application.Quit());
    }

    /// <summary>
    /// 뒤끝 서버 연결
    /// </summary>
    void ServerConnect()
    {
        var bro = Backend.Initialize(); //뒤끝 초기화

        //뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            //유저 로그인
            Backend.BMember.CustomLogin("user1", "1234", callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("로그인 성공");
                    Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            EndDownload();
                            GameData.LicenseTable = callback.FlattenRows(); //라이선스 데이터 수집
                            UpdateCheck();
                        }
                        else
                        {
                            ServerConnect();
                        }
                    });
                }
            });
        }
    }

    /// <summary>
    /// 버전 체크를 통하여 업데이트를 해야하는지 확인
    /// </summary>
    private void UpdateCheck()
    {
        Backend.GameData.GetMyData("VersionData", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                string currentVersion = callback.FlattenRows()[0]["Version"].ToString(); //현재 최신 버전
                string appVersion = Application.version;                                 //현재 설치된 버전
                //Debug.Log($"currentVersion : {currentVersion} / appVersion : {appVersion}");

                //현재 설치된 버전과 최신버전을 비교
                if (currentVersion == appVersion) 
                {
                    //데이터 다운로드 시작
                    FindObjectOfType<DownManager>().Init();                    
                }
                else
                {
                    //업데이트 안내 팝업 실행
                    Popup_Open(Popup_Update); 
                }
            }
            else
            {
                //연결이 끊겼을 때 로그인하여 재시도
                var bro = Backend.Initialize();
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            UpdateCheck();
                        }
                    });
                }
            }
        });
    }


    /// <summary>
    /// 테이블 데이터 다운로드
    /// </summary>
    void DownloadTableData()
    {
        StartDownload();
        Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                EndDownload();
                GameData.LicenseTable = callback.FlattenRows();
            }
            else
            {
                //다른기기에서 로그인 했을때
                var bro = Backend.Initialize();
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            DownloadTableData();
                        }
                    });
                }
            }
        });
    }

    /// <summary>
    /// 라이선스 등록 여부에 따라 UI오픈
    /// </summary>
    public void OpenTeacherMode()
    {
        StartDownload();
        Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                EndDownload();
                GameData.LicenseTable = callback.FlattenRows();

                Debug.Log("라이선스 등록여부 확인");

                if (GameData.RegistryDataCheck())
                {
                    roomCodeMaker.SetActive(true);
                }
                else
                {
                    Input_License.text = string.Empty; 
                    OpenInputField_LicenseNumber();
                    LicenseRegister.SetActive(true);
                }
            }
            else
            {
                //다른기기에서 로그인 했을때
                var bro = Backend.Initialize();
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            OpenTeacherMode();
                        }
                    });
                }
            }
        });
    }

    #region 라이선스 입력
    /// <summary>
    /// 라이선스 제출 버튼을 눌렀을 때
    /// </summary>
    void CheckLicense()
    {
        //데이터 갱신
        StartDownload();
        Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                EndDownload();
                GameData.LicenseTable = callback.FlattenRows();

                //입력받은 라이선스 번호
                licenseNumber = Input_License.text.ToUpper();
                //유효한 라이선스 번호이고 사용중이지 않을때        
                if (GameData.LicenseCheck(licenseNumber))
                {
                    //등록할 라이선스의 기본 정보 수집
                    GameData.GetUserData(licenseNumber);

                    //사용할 기관명 입력 UI 활성화
                    OpenInputField_SchoolName();
                }
                else
                {
                    //유효한 라이선스 번호가 아니거나 이미 사용중인 라이선스임을 알려주는 UI 팝업
                    Popup_L.SetActive(true);
                }
            }
            else
            {
                //다른기기에서 로그인 했을때
                var bro = Backend.Initialize();
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            CheckLicense();
                        }
                    });
                }
            }
        });        
    }
    
    /// <summary>
    /// 라이선스 입력 UI 활성화
    /// </summary>
    void OpenInputField_LicenseNumber()
    {
        Input_License.text = "";
        Popup_Open(obj_licenseUI);
        Popup_Close(obj_schoolNameUI);
    }
    #endregion

    #region 라이선스 찾기
    /// <summary>
    /// 백업코드를 이용해 라이선스 번호를 불러옴
    /// </summary>
    void Check_backupCode()
    {
        StartDownload();
        Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                EndDownload();
                GameData.LicenseTable = callback.FlattenRows();

                string licenseNum = GameData.GetLicenseNumber(Input_BackupCode.text.ToUpper());

                if (licenseNum != null)
                {
                    Text_FindResult.text = $"라이선스 번호는\r\n{licenseNum}\r\n입니다.";
                }
                else
                {
                    Text_FindResult.text = "일치하는 라이선스가 없습니다.";
                }

                Popup_Open(Popup_FindResult);
                Popup_Close(Popup_FindLicense);
            }
            else
            {
                //다른기기에서 로그인 했을때
                var bro = Backend.Initialize();
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            Check_backupCode();
                        }
                    });
                }
            }
        });
    }
    #endregion

    #region 라이센스 등록 정보 초기화
    void Check_ResetCode()
    {
        string license = Input_CancelLicense.text.ToUpper();

        GameData.GetUserData(license);         //등록을 해제할 라이선스의 유저정보
        UserData data = GameData.GetRowData();

        //활성화 된 라이선스일때
        if (data.ActiveCheck)
        {
            //서버 데이터 초기화
            GameData.DeleteRowData();

            //내장 데이터 초기화
            PlayerPrefs.SetInt("bActivate", 0);                   
            PlayerPrefs.SetString("License", "");
            PlayerPrefs.SetString("SchoolName", ""); 
            PlayerPrefs.SetString("inDate", "");

            PlayerPrefs.Save();

            Text_CancelResult.text = "다른 기기의 인증이 해제되었습니다.\r\n현재 기기에 라이선스를 등록할 수 있습니다.";    
            
            DownloadTableData();
        }        
        else
        {
            Text_CancelResult.text = "등록되지 않은 라이선스이거나\r\n유효하지 않은 라이선스입니다.";
        }

        Popup_Close(Popup_CancelLicense); //입력창 닫기
        Popup_Open(Popup_CancelResult);   //결과창 열기
    }
    #endregion

    #region 학교명 등록
    /// <summary>
    /// 학교명 입력 UI 활성화. UI 변경
    /// </summary>
    void OpenInputField_SchoolName()
    {
        Input_SchoolName.text = "";
        Popup_Open(obj_schoolNameUI);
        Popup_Close(obj_licenseUI);
    }

    /// <summary>
    /// 학교명에 불순한 단어가 있는지 확인
    /// </summary>
    /// <param name="nickname">입력된 학교 명</param>
    /// <returns></returns>
    private bool NameFiltering(string nickname)
    {
        //공백
        if (string.IsNullOrEmpty(nickname)) return false; 

        //부적절한 단어
        for (int i = 0; i < slang.Count; i++) 
        {
            if (nickname.Contains(slang[i]["CommonSlang"].ToString()))
            {                
                return false;
            }
        }

        //문제 없음
        return true;
    }

    /// <summary>
    /// 학교 이름 제출 버튼을 눌렀을 때 
    /// 비속어 탐지 기능 추가 필요
    /// 중첩되는 것은 상관없음
    /// </summary>
    void CheckSchoolName()
    {
        //입력받은 학교명
        schoolName = Input_SchoolName.text;

        //이름에 문제가 없을 때
        if (NameFiltering(schoolName)) 
        {
            Popup_Conrim.SetActive(true);
            Text_Confirm.text = $"등록 이후엔 변경할 수 없습니다.\r\n'<color=#7650FF><b>{schoolName}</b></color>'\r\n(으)로 등록하시겠습니까?";
        }
        //공백이거나 부적절한 단어가 섞여 있을 때
        else 
        {
            Popup_Open(Popup_BadName);
        }
    }

    /// <summary>
    /// 선택한 이름으로 확정. 데이터 업데이트 진행
    /// </summary>
    void Btn_NameOk()
    {
        GameData.UserDataUpdate(schoolName);   //데이터 업로드
        DownloadTableData();

        UserData data = GameData.GetRowData(); //유저정보 수집

        Text_End.text = $"등록이 완료되었습니다.\r\n- 백업코드 -\r\n{data.BackupCode}";

        //PlayerPrefs에 데이터 저장★
        PlayerPrefs.SetInt("bActivate", 1);                   //true라서 1로 저장
        PlayerPrefs.SetString("License", data.LicenseNumber); 
        PlayerPrefs.SetString("SchoolName", schoolName);      //입력받은 학교명
        PlayerPrefs.SetString("inDate",data.Indate);

        PlayerPrefs.Save();        

        Popup_Close(Popup_Conrim);  //확인 안내 UI 닫기
        Popup_Open(Popup_End);      //마지막 UI 활성화
    }
    #endregion

    /// <summary>
    /// 직업선택 페이지로 이동
    /// </summary>
    public void BackToJobPage()
    {   
        LicenseRegister.SetActive(false);
        roomCodeMaker.SetActive(false);
        obj_job.SetActive(true);

        // 2025-03-19 RJH 만약 방에 참여 중이면 방 나가기
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
        
    /// <summary>
    /// 발급받은 방 입장코드를 뒤끝서버에 등록
    /// </summary>
    /// <param name="code"></param>
    public void GetRoomCode()
    {
        StartDownload();
        Backend.GameData.GetMyData("LicenseTable", new Where(), callback =>
        {
            if (callback.IsSuccess())
            {
                EndDownload();
                GameData.LicenseTable = callback.FlattenRows();

                roomCodeMaker.GetComponent<RoomCodeMaker>().SetRoomCode(GameData.UploadRoomCode());
            }
            else
            {
                Debug.LogError("게임 정보 조회에 실패했습니다. : " + callback + "재시도");
                var bro = Backend.Initialize();    
                if (bro.IsSuccess())
                {
                    Backend.BMember.CustomLogin("user1", "1234", callback =>
                    {
                        if (callback.IsSuccess())
                        {
                            GetRoomCode();
                        }
                    });
                }
            }
        });
    }

    #region 팝업 관리
    /// <summary>
    /// 팝업 열기
    /// </summary>
    /// <param name="obj"></param>
    void Popup_Open(GameObject obj)
    {
        obj.SetActive(true);
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    /// <param name="obj"></param>
    void Popup_Close(GameObject obj)
    {
        obj.SetActive(false);
    }

    /// <summary>
    /// 데이터 갱신시 생성할 팝업 열기
    /// </summary>
    public void StartDownload()
    {
        //Debug.Log("데이터 업데이트");
        Popup_Open(Popup_Connecting);
    }
    public void EndDownload() 
    {
        //Debug.Log("데이터 업데이트 완료");
        Popup_Close(Popup_Connecting);
    }
    #endregion
}