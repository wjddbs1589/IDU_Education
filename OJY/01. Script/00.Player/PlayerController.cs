using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerController : MonoBehaviour
{
    float RotateSpeed = 90.0f;
    float MoveSpeed = 25.0f;

    Ray ray;
    RaycastHit hit;

    GameObject FirstObj;
    GameObject SecondObj;

    GameObject PlayerObj;

    [SerializeField]
    GameObject Cloth_Canvas;

    void Update()
    {
        Move();
        TargetCheck();
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += (transform.forward * MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position += (transform.forward * -MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Quaternion rotY = Quaternion.Euler(0, -RotateSpeed * Time.deltaTime, 0);
            transform.rotation *= rotY;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Quaternion rotY = Quaternion.Euler(0, RotateSpeed * Time.deltaTime, 0);
            transform.rotation *= rotY;
        }
    }

    /// <summary>
    /// 입력이 들어갔을 때 눌렀을 때 가리키는 대상와 뗄 때 가리키는 대상이 같은지 확인하여 상호작용
    /// </summary>
    void TargetCheck()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.F)) 
        {
            FirstObj = null; //초기화

            if (Physics.Raycast(ray, out hit))
            {
                FirstObj = hit.collider.gameObject;
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            SecondObj = null; //초기화

            if (Physics.Raycast(ray, out hit))
            {
                SecondObj = hit.collider.gameObject;
            }

            if ((FirstObj && SecondObj) && FirstObj == SecondObj) //두 오브젝트가 null이 아니고 같은 대상일때
            {
                InteractiveNPC();
            }

            //End_Player_Custom();
        }
    }
    

    void InteractiveNPC() 
    {
        NPC npc01 = hit.collider.GetComponent<NPC>();

        if (npc01)
        {
            npc01.NpcInteractive();
        }

        Start_Player_Custom();
    }

    void Start_Player_Custom()
    {
        if (!Cloth_Canvas) return;
        Cloth_Canvas.SetActive(true);


        PlayerObj = transform.GetChild(0).gameObject;

        if (!PlayerObj) return;

        PlayerObj.transform.localPosition = new Vector3(-1.0f, 0.0f, 2.0f);
        PlayerObj.transform.LookAt(Camera.main.transform);
    }

    void End_Player_Custom()
    {
        if (!Cloth_Canvas) return;
        Cloth_Canvas.SetActive(false);

        if (!PlayerObj) return;

        PlayerObj.transform.localPosition = Vector3.zero;
        PlayerObj.transform.rotation = Quaternion.identity;
    }
}
