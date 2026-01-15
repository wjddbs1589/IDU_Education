using BackEnd;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LicenseData
{
    private static LicenseData _instance = null;
    public static LicenseData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LicenseData();
            }

            return _instance;
        }
    }

    public bool ActiveCheck;
    public string LicenseNumber;
    public string SchoolName;
    public string BackupCode;
    public string RoomCode;

    LitJson.JsonData TableData;

    public Param ToParam()
    {
        Param param = new Param();

        param.Add("ActiveCheck", ActiveCheck);
        param.Add("BackupCode", BackupCode);
        param.Add("LicenseNumber", LicenseNumber);
        param.Add("RoomCode", RoomCode);
        param.Add("SchoolName", SchoolName);
        return param;
    }

    /// <summary>
    /// 데이터 테이블 정보 수집
    /// </summary>
    public void GetTableData()
    {
        var bro = Backend.GameData.GetMyData("LicenseTable", new Where());

        if (bro.IsSuccess())
        {
            TableData = bro.FlattenRows();
            Debug.Log("Debug : Success - " + bro.GetReturnValuetoJSON());
        }
        else
        {
            Debug.Log("Debug : Fail - " + bro.GetStatusCode());
            Debug.Log("Debug : Error - " + bro.GetErrorCode());
            Debug.Log("Debug : Message - " + bro.GetMessage());
        }
    }

    /// <summary>
    /// 중복되는 라이선스 검사
    /// </summary>
    /// <returns>사용가능 여부</returns>
    public bool LicenseCheck()
    {
        bool canUse = true;

        //곂치는게 있으믄 false반환
        foreach (LitJson.JsonData data in TableData)
        {
            Debug.Log($"생성된 번호 : {LicenseNumber} / 기존 번호 : {data["LicenseNumber"].ToString()}");
            if (LicenseNumber == data["LicenseNumber"].ToString())
            {
                Debug.Log("중복사항 있음");
                return false;
            }
        }

        Debug.Log("중복사항 없음");
        return canUse;
    }
}
public class LicenseMaker : MonoBehaviour
{
    LicenseData data;
    [SerializeField] GameObject LoadingImage;

    [SerializeField] TMP_InputField text;
    [SerializeField] TextMeshProUGUI uploadInfoText;

    [SerializeField] Button MakeBtn;
    [SerializeField] Button UploadBtn;
    [SerializeField] Button ExitBtn;

    IEnumerator InfoCo;
    private void Awake()
    {
        MakeBtn.onClick.AddListener(CreateLicenseNumber);
        UploadBtn.onClick.AddListener(LicenseUpload);
        ExitBtn.onClick.AddListener(() => Application.Quit());
        uploadInfoText.text = "";
    }
    // Start is called before the first frame update
    void Start()
    {
        StartServer();
    }

    /// <summary>
    /// 뒤끝 서버 연결
    /// </summary>
    void StartServer()
    {
        data = LicenseData.Instance; //인스턴스 저장
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success

            Backend.BMember.CustomLogin("user1", "1234", callback =>
            {
                if (callback.IsSuccess())
                {
                    if (Backend.IsLogin) //로그인 상태 최종 확인
                    {
                        data.GetTableData();
                        LoadingImage.SetActive(false);
                    }
                }
            });

        }
        else
        {   
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
            StartServer();
        }
    }

    /// <summary>
    /// 라이선스 코드 생성
    /// </summary>
    void CreateLicenseNumber()
    {
        //백업코드 생성
        string backupCode = DateTime.Now.Ticks.ToString() + UnityEngine.Random.Range(1000, 9999); //시간 + 랜덤 값
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(backupCode));
            data.LicenseNumber = BitConverter.ToString(hash).Replace("-", "").Substring(0, 16); //16자리 해시 값 반환
            if (!data.LicenseCheck()) CreateLicenseNumber(); //중복사항이 있으면 재생성
        }
        text.text = data.LicenseNumber;
        UploadBtn.interactable = true;
    }

    /// <summary>
    /// 생성된 라이선스를 뒤끝에 등록
    /// </summary>
    void LicenseUpload()
    {
        data = new LicenseData();
        data.ActiveCheck = false;
        data.BackupCode = "";
        data.LicenseNumber = text.text;
        data.RoomCode = "";
        data.SchoolName = "";

        Param param = data.ToParam();

        var bro = Backend.GameData.Insert("LicenseTable", param);

        if (bro.IsSuccess())
        {
            string playerInfoIndate = bro.GetInDate();
            uploadInfoText.text = "등록이 완료되었습니다.";
            Debug.Log("내 playerInfo의 indate : " + playerInfoIndate);
        }
        else
        {
            uploadInfoText.text = "등록에 실패했습니다.";
            Debug.LogError("게임 정보 삽입 실패 : " + bro.ToString());
        }
        
        data.GetTableData();

        if(InfoCo != null) StopCoroutine(InfoCo);

        InfoCo = TextCo();
        UploadBtn.interactable = false;
        StartCoroutine(InfoCo);
    }

    IEnumerator TextCo()
    {
        yield return new WaitForSeconds(3.0f);
        uploadInfoText.text = "";
    }
}
