using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class NPC_UI : MonoBehaviour
{
    //Transform mainCamera;
    //float distance;

    // Start is called before the first frame update
    void Start()
    {
        //mainCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        ////가까워 졌을때 효과(아웃라인, 상호작용 버튼 UI 표시 등) 켜기
        //if (distance < 10.0f)
        //{
        //    // 현재 오브젝트의 위치
        //    Vector3 objectPosition = transform.position;

        //    // 카메라 위치
        //    Vector3 cameraPosition = mainCamera.position;

        //    // Y축만 회전하도록 카메라의 Y 좌표를 현재 오브젝트의 Y 좌표로 고정
        //    cameraPosition.y = objectPosition.y;

        //    // 목표 회전 값 계산
        //    Quaternion targetRotation = Quaternion.LookRotation(cameraPosition - objectPosition);

        //    // 현재 회전을 목표 회전으로 Lerp
        //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        //}
    }
}
