using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    //플레이어와의 거리
    float Distance;
    bool bOpen;

    //UI표시 여부
    GameObject ui;
    bool bEffectOn;
    NPC_UI npc_UI;

    //테스트용, 머터리얼 및 색상
    Material mat;
    Color OriginColor;

    void Start()
    {
        bOpen = false;
        bEffectOn = false;

        npc_UI = GetComponentInChildren<NPC_UI>();
        if (npc_UI) 
        {
            ui = npc_UI.gameObject;
            ui.SetActive(false);
        }
        
        mat = GetComponent<Renderer>().material;
        OriginColor = mat.color;
    }

    void Update()
    {
        CheckDistance();
    }

    /// <summary>
    /// 플레이어와의 거리 체크
    /// </summary>
    void CheckDistance()
    {
        //거리계산
        Distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        //거리에 따른 상호작용 UI 표시        
        if (Distance > 10.0f)
        {
            Off_UI();
        }
        else if (Distance <= 10.0f)
        {
            On_UI();
        }
    }

    /// <summary>
    /// 유저가 가까워졌을 때 UI ON
    /// </summary>
    void On_UI() 
    {
        if (bEffectOn) return; //이미 효과가 켜져있으면 return

        bEffectOn = true;

        //-----특수효과-----
        if (npc_UI)
        {            
            ui.SetActive(true);
        }
    }

    /// <summary>
    /// 유저가 멀어졌을 때 UI off
    /// </summary>
    void Off_UI()
    {
        if (!bEffectOn) return; //이미 효과가 꺼져있으면 return

        bEffectOn = false;

        //-----특수효과-----
        if (npc_UI)
        {
            ui.SetActive(false);
        }

        //멀어졌을때 자동으로 상호작용 종료
        if (bOpen)
        {
            mat.color = OriginColor;
            bOpen = false;
        }
    }

    /// <summary>
    /// 근거리에서 상호작용 했을때 상호작용
    /// </summary>
    public void NpcInteractive()
    {
        if (Distance < 10.0f)
        {
            if (!bOpen)
            {
                mat.color = Color.red;
                bOpen = true;
            }
            else
            {
                mat.color = OriginColor;
                bOpen = false;
            }
        }
    }
}