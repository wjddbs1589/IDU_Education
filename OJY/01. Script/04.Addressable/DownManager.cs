using BackEnd;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DownManager : MonoBehaviour
{
    [Header("직업선택 UI")]
    [SerializeField] GameObject JobSelectBackground; //다운로드 완료 후 보여줄 직업 선택 UI

    [Header("업데이트 UI")]
    [SerializeField] GameObject WaitMessage;         //확인 중 메시지 (대기 중 표시용)
    [SerializeField] GameObject DownMessage;         //다운로드가 필요할 때 보여줄 메시지

    [SerializeField] TextMeshProUGUI fileSizeText;   //다운로드해야 할 파일의 용량 표시
    [SerializeField] GameObject sliderObj;           //다운로드 진행률 표시 슬라이더 객체
    Slider slider;
    [SerializeField] TextMeshProUGUI percentText;    //다운로드 진행 퍼센트 텍스트

    [Header("Label")]
    [SerializeField] AssetLabelReference defaultLabel; //어드레서블 다운로드 대상 라벨

    long patchSize; //전체 다운로드해야 할 바이트 크기
    Dictionary<string, long> patchMap = new Dictionary<string, long>(); //라벨별 다운로드 진행상황 저장용

    [Header("Button")]
    [SerializeField] Button Btn_Down; //다운로드 시작 버튼

    [Header("새로운 버전 확인")]
    int playingBundleVersion; //현재 실행중인 앱의 번들 버전 값
    int recentBundleVersion;  //최신 번들 버전 값
    [SerializeField] GameObject NewVersionPopup;



    private void Awake()
    {
        Btn_Down.onClick.AddListener(Btn_Download);
        slider = sliderObj.GetComponentInChildren<Slider>();
    }

    void Start()
    {
        //초기 UI 상태 설정
        JobSelectBackground.SetActive(false); //직업 선택 UI는 숨김
        WaitMessage.SetActive(true);          //"업데이트 확인 중" 메시지 표시
        DownMessage.SetActive(false);         //다운로드 안내 메시지는 숨김
        sliderObj.SetActive(false);           //슬라이더 숨김
    }

    #region 번들파일 다운로드    
    /// <summary>
    /// 다운로드 시작
    /// </summary>
    public void Init()
    {
        StartCoroutine(CheckUpdate());
    }

    //Addressables 초기화 및 다운로드 필요 여부 확인
    IEnumerator CheckUpdate()
    {
        yield return Addressables.InitializeAsync(); //Addressables 초기화

        var label = defaultLabel.labelString;
        var sizeHandle = Addressables.GetDownloadSizeAsync(label); //해당 라벨의 다운로드 크기 요청
        yield return sizeHandle;

        patchSize = sizeHandle.Result; //다운로드해야 할 총 용량
        //Debug.Log(patchSize);

        if (patchSize > 0)
        {   
            //다운로드할 리소스가 있는 경우
            WaitMessage.SetActive(false);
            DownMessage.SetActive(true);
            fileSizeText.text = $"파일 사이즈: {GetFileSize(patchSize)}"; //용량 표시
        }
        else
        {
            //다운로드할 리소스가 없는 경우 (이미 있음)
            percentText.text = "100%";
            slider.value = 1.0f;

            JobSelectBackground.SetActive(true); //다음 UI 열기
            gameObject.SetActive(false);         //이 매니저는 숨김
        }
    }
    //바이트 → KB, MB, GB 문자열 변환
    string GetFileSize(long byteCnt)
    {
        if (byteCnt >= 1073741824) return $"{byteCnt / 1073741824.0:0.##} GB";
        if (byteCnt >= 1048576) return $"{byteCnt / 1048576.0:0.##} MB";
        if (byteCnt >= 1024) return $"{byteCnt / 1024.0:0.##} KB";
        return $"{byteCnt} Bytes";
    }

    //다운로드 버튼 클릭 시 호출
    void Btn_Download()
    {
        DownMessage.SetActive(false);         //버튼 숨김
        sliderObj.SetActive(true);            //슬라이더 표시
        StartCoroutine(DownloadWithProgress()); //다운로드 진행 코루틴 실행
    }
    //다운로드 진행 상태를 실시간으로 업데이트
    IEnumerator DownloadWithProgress()
    {
        var label = defaultLabel.labelString;
        patchMap[label] = 0;

        var handle = Addressables.DownloadDependenciesAsync(label, false); //비동기 다운로드 요청 (자동 캐싱 X)
        while (!handle.IsDone)
        {
            var status = handle.GetDownloadStatus(); //현재 다운로드 상태 정보
            patchMap[label] = status.DownloadedBytes;

            //전체 퍼센트 계산 (현재 다운로드된 용량 / 전체 다운로드 용량)
            slider.value = (float)patchMap.Values.Sum() / patchSize;
            percentText.text = $"{(int)(slider.value * 100)}%";

            yield return new WaitForEndOfFrame(); //다음 프레임까지 대기
        }

        //다운로드 완료 후 상태 확인
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"다운로드 실패: {handle.OperationException?.Message}");
            percentText.text = "오류 발생";
            yield break; //실패 시 종료
        }

        //완료된 상태로 퍼센트 100% 설정
        patchMap[label] = handle.GetDownloadStatus().TotalBytes;
        slider.value = 1f;
        percentText.text = "100%";
        Addressables.Release(handle); //핸들 해제

        JobSelectBackground.SetActive(true); //다음 UI 열기
        gameObject.SetActive(false);         //이 매니저 비활성화
    }    
    #endregion
}
