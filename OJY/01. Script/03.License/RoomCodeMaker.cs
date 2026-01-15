using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCodeMaker : MonoBehaviour
{
    LicenseManager licenseManager;
    PhotonManager photonManager;

    int userCount = 30;

    [Header("유저수 조정")]
    [SerializeField]
    TMP_InputField UserCountText; 

    [SerializeField]
    Button Btn_Pluse;

    [SerializeField]
    Button Btn_Minus;

    [Header("입장 코드 조정")]
    [SerializeField]
    TMP_InputField RoomCodeText;

    [SerializeField]
    Button Btn_CreateCode;

    string roomCode;

    [Header("버튼")]
    [SerializeField]
    Button Btn_Exit;

    [SerializeField]
    Button Btn_Next;

    [Header("로비매니저")]
    [SerializeField] GameObject Obj_lobby;
    LobbyManager lobbyManager;

    [Header("캐릭터 생성 버튼")]
    [SerializeField] GameObject NextBtn;

    void Awake()
    {
        licenseManager = transform.parent.GetComponent<LicenseManager>();
        lobbyManager = Obj_lobby.GetComponent<LobbyManager>();
        photonManager = FindObjectOfType<PhotonManager>();

        Btn_CreateCode.onClick.AddListener(CreateRoomCode);
        Btn_Pluse.onClick.AddListener(Plus_userCount);
        Btn_Minus.onClick.AddListener(Minus_userCount);

        Btn_Exit.onClick.AddListener(ExitRoom);
        Btn_Next.onClick.AddListener(() => { lobbyManager.BackToNamePage(); gameObject.SetActive(false);}); 
    }

    void Plus_userCount()
    {
        if (userCount < 50)
        {
            userCount += 1;
            UserCountText.text = userCount.ToString();
        }        
    }

    void Minus_userCount()
    {
        if (userCount > 10)
        {
            userCount -= 1;
            UserCountText.text = userCount.ToString();
        }
    }

    void ExitRoom()
    {
        Btn_CreateCode.interactable = true; 
        NextBtn.SetActive(false);
        RoomCodeText.text = "";
        licenseManager.BackToJobPage(); 
    }

    /// <summary>
    /// 입장코드 생성
    /// </summary>
    void CreateRoomCode()
    {
        //입장코드 업로드
        licenseManager.GetRoomCode();
    }

    public void SetRoomCode(string code)
    {
        RoomCodeText.text = code;

        //포톤에 입장코드 등록
        photonManager.StartCreateLobbyRoom(code, userCount);

        Btn_CreateCode.interactable = false;
        NextBtn.SetActive(true);
    }
}
