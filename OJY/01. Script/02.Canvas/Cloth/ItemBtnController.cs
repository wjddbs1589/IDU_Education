using UnityEngine;
using UnityEngine.UI;

public class ItemBtnController : MonoBehaviour
{
    [SerializeField] Sprite NormalImage;
    [SerializeField] Sprite ClickedImage;

    Image image_btn;
    CanvasGroup group;

    private void Awake()
    {
        image_btn = GetComponent<Image>();
        group = GetComponent<CanvasGroup>();    
    }

    public void NormalSetting()
    {
        image_btn.sprite = NormalImage;
        group.alpha = 0.95f;
    }
    public void ClickedSetting()
    {
        image_btn.sprite = ClickedImage;
        group.alpha = 1.0f;
    }
}
