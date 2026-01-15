using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBtnController : MonoBehaviour
{
    [SerializeField] Sprite NormalImage;
    [SerializeField] Sprite ClickedImage;

    Image image_btn;

    private void Awake()
    {
        image_btn = GetComponent<Image>();
    }

    public void Set_NormalImage() 
    {
        if(!image_btn) image_btn = GetComponent<Image>();

        image_btn.sprite = NormalImage;
    }
    public void Set_ClickedImage() 
    {
        image_btn.sprite = ClickedImage;
    }
}
