using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BackEnd;
using Photon.Pun.UtilityScripts;
public struct Quiz 
{
    public string Question;
    public bool Answer;
    public string Information;

    //퀴즈의 문제와 답을 저장할 함수
    public Quiz(string question, bool answer, string info)
    {
        this.Question = question;
        this.Answer = answer;
        this.Information = info;
    }
}

public class QuizController : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    List<Quiz> quizList;                           //퀴즈가 저장될 리스트
    [SerializeField] TextMeshProUGUI questionText; //퀴즈의 문제
    bool answer;                                   //퀴즈의 정답
    string information;                            //퀴즈의 설명
    int quizNum;                                   //현재 퀴즈 번호
    [SerializeField] TextMeshProUGUI quizNumber;   //퀴즈번호 텍스트
    int questionCount;                             //퀴즈 갯수

    string[] arr_question; //문제 배열
    bool[] arr_answer;     //정답 배열
    string[] arr_info;     //설명 배열
    
    [Header("Btn_OX")]
    [SerializeField] Button Btn_O; //O 버튼
    [SerializeField] Button Btn_X; //X 버튼
    bool myAnswer;                 //나의 답
    
    [Header("해설 텍스트")]
    TextMeshProUGUI InformationText;

    [Header("정답 팝업")]
    [SerializeField] GameObject AnswerBoard; //정답 결과창
    [SerializeField] Button Btn_CloseBoard;  //결과창 닫기
    [SerializeField] Image AnswerImage;      //문제의 정답을 표기할 이미지
    [SerializeField] Sprite AnswerSprite;
    [SerializeField] Sprite WrongSprite;

    [Header("정답 타이틀")]
    [SerializeField] GameObject[] Titles = new GameObject[3]; //정답 보드 타이틀
    
    [Header("상단 타이머")]
    [SerializeField] TextMeshProUGUI timer;
    int sec;
    int msec;
    float countdownTime = 10f;
    bool bStopTimer = true;
    
    [Header("종료알림 팝업")]
    [SerializeField] GameObject PopupObj;
    [SerializeField] Button EndBtn;
    
    [Header("종료알림 팝업")]
    [SerializeField] GameObject QuizObj;
    [SerializeField] GameObject StartObj;
    [SerializeField] Button StartBtn;
    [SerializeField] Button CancelBtn;

    [Header("상단 UI")]
    [SerializeField] GameObject object_UI;
    private void Awake()
    {
        //버튼 바인딩
        Btn_O.onClick.AddListener( () => { myAnswer = true; AnswerCheck(); });
        Btn_X.onClick.AddListener( () => { myAnswer = false; AnswerCheck(); });
        EndBtn.onClick.AddListener(EndQuiz);

        //결과창 닫기 바인딩
        Btn_CloseBoard.onClick.AddListener(Close_ResultBoard);
        InformationText = AnswerBoard.GetComponentInChildren<TextMeshProUGUI>();

        //시작 팝업 관리
        StartBtn.onClick.AddListener(StartGame);
        CancelBtn.onClick.AddListener(() => StartObj.SetActive(false));       

        bStopTimer = true;
        quizNum = 0;
    }

    private void Start()
    {
        // 2025-03-28 RJH 선생님의 경우 퀴즈 X
        if (GameManager.Instance.job == JOB.TEACHER)
        {
            this.gameObject.SetActive(false);
            return;
        }

        var backendInit = Backend.Initialize(); //뒤끝 초기화
        GameManager.Instance.CameraControl(false);
        //뒤끝 초기화에 대한 응답값
        if (backendInit.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + backendInit); //성공일 경우 statusCode 204 Success                                                  
            object_UI.SetActive(false);
            GetQuizData(); //초기 문제 세팅
        }
        else
        {
            Debug.LogError("초기화 실패 : " + backendInit); //실패일 경우 statusCode 400대 에러 발생
        }        
    }

    private void Update()
    {
        if (bStopTimer) return;

        // 시간이 0 이하일 때 타이머가 멈추게 하기
        if (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;  // 매 프레임마다 남은 시간만큼 감소

            // sec와 msec 값을 계산 (초와 밀리초로 나누기)
            sec = Mathf.FloorToInt(countdownTime);  // 남은 시간의 초 부분
            msec = Mathf.FloorToInt((countdownTime - sec) * 100);  // 남은 시간의 밀리초 부분 (100을 곱해 소수점 이하 두 자리)

            timer.text = $"{sec:00} : {msec:00}";

            if (sec < 4)
            {
                timer.text = $"<color=#FF0000>{timer.text}</color>";
            }
            else
            {
                timer.text = $"<color=#000000>{timer.text}</color>";
            }
        }
        else
        {
            timer.text = "<color=#FF0000>00 : 00</color>";  //타이머가 0이 되면 "00 : 00" 표시
            AnswerCheck();
        }
    }

    /// <summary>
    /// 뒤끝 접속 및 정보 가져오기
    /// </summary>
    public void GetQuizData()
    {
        Backend.BMember.CustomLogin("user1", "1234", (callback) =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("수동 로그인 성공!");
                GetDataTable("172516"); //QuizData.csv 차트 파일 ID코드를 뒤끝에서 확인해서 입력
            }
            else
            {
                Debug.LogError("로그인 실패! : " + callback.GetMessage());
                GetQuizData();
            }
        });
    }

    /// <summary>
    /// Quiz의 정보를 가져오고 초기 문제 세팅
    /// </summary>
    /// <param name="ChardID"></param>
    void GetDataTable(string ChardID)
    {
        //퀴즈 리스트 생성
        quizList = new List<Quiz>();

        // 파일 가져오기
        var QuizChart = Backend.Chart.GetChartContents(ChardID);

        //파일을 가져오는데 실패했으면 함수종료
        if (!QuizChart.IsSuccess()) return;

        questionCount = QuizChart.FlattenRows().Count; //총 문제 갯수 

        //문재 갯수만큼 배열 초기화
        arr_question = new string[questionCount];
        arr_answer = new bool[questionCount];
        arr_info = new string[questionCount];

        int QuizIndex = 0;
        //데이터 파싱 및 저장
        foreach (LitJson.JsonData Category in QuizChart.FlattenRows())
        {
            arr_question[QuizIndex] = Category["Question"].ToString();
            arr_answer[QuizIndex] = bool.Parse(Category["Answer"].ToString());
            arr_info[QuizIndex] = Category["Info"].ToString();

            AddQuiz(arr_question[QuizIndex], arr_answer[QuizIndex], arr_info[QuizIndex]);
            QuizIndex++;
        }

        SelectQuiz();
    }

    /// <summary>
    /// 퀴즈 문항 추가
    /// </summary>
    /// <param name="q">문제</param>
    /// <param name="a">정보</param>
    /// <param name="i">해설</param>
    void AddQuiz(string q, bool a, string i)
    {
        quizList.Add(new Quiz(q, a, i));
    }

    /// <summary>
    /// 랜덤으로 퀴즈중에 1개 선택
    /// </summary>
    void SelectQuiz()
    {
        //10문제를 다 풀었으면
        if (quizNum >= 10)
        {
            PopupObj.SetActive(true);
        }
        else
        {
            int QuizNumber = Random.Range(0, quizList.Count);//랜덤으로 퀴즈 선택
            SetQuiz(quizList[QuizNumber]);                   //퀴즈내용 적용
            quizList.Remove(quizList[QuizNumber]);           //사용한 퀴즈 삭제
        }
    }

    /// <summary>
    /// 선택된 퀴즈 적용
    /// </summary>
    /// <param name="quiz">선택된 퀴즈 구조체</param>
    void SetQuiz(Quiz quiz)
    {
        quizNum++;
        quizNumber.text = $"퀴즈 {quizNum} / 10";

        questionText.text = quiz.Question;
        answer = quiz.Answer;
        information = quiz.Information;
        
        //제한시간 초기화
        countdownTime = 10;
        sec = 10;
        msec = 0;
        timer.text = $"{sec:00} : {msec:00}";
    }

    /// <summary>
    /// 버튼을 눌렀을 때 정답 확인
    /// </summary>
    void AnswerCheck()
    {
        bStopTimer = true;

        if (sec <= 0) 
        {
            SetAnswerTitle(2);
        }
        else
        {
            //내 답과 정답이 같을 때
            if (myAnswer == answer)
            {
                SetAnswerTitle(0);
            }
            else
            {
                SetAnswerTitle(1);
            }
        }

        //정답에 따른 이미지 변경
        AnswerImage.sprite = answer ? AnswerSprite : WrongSprite;
        //해설 내용 변경
        InformationText.text = information;

        AnswerBoard.SetActive(true);
    }


    /// <summary>
    /// 정답 여부에 따른 타이틀 이미지 변경
    /// </summary>
    /// <param name="index">0 = 정답 / 1 = 오답 / 2 = 시간초과</param>
    void SetAnswerTitle(int index)
    {
        foreach (GameObject obj in Titles)
        {
            obj.SetActive(false);
        }

        Titles[index].SetActive(true);
    }

    /// <summary>
    /// 정답 결과창 닫기
    /// </summary>
    void Close_ResultBoard()
    {
        //결과창 닫기
        AnswerBoard.SetActive(false);

        if (quizNum >= 10)
        {
            bStopTimer = true;
        }
        else
        {
            bStopTimer = false;
        }

        //새로운 문제 등록
        SelectQuiz();
    }

    /// <summary>
    /// OX퀴즈 종료 팝업 닫기 + 이후 이벤트 실행
    /// </summary>
    public void EndQuiz()
    {
        GameManager.Instance.CameraControl(true);

        //직업에 따른 수료식 진행
        // 2025-03-27 RJH 수료식 진행
        GameManager.Instance.StartGraduationScenario();

        object_UI.SetActive(true);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 시작팝업의 '네' 버튼을 눌렀을 때
    /// </summary>
    void StartGame()
    {
        StartObj.SetActive(false);
        QuizObj.SetActive(true);
        bStopTimer = false;
    }
}
