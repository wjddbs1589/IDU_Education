using UnityEngine;
using UnityEngine.UI;

public class BtnClothSprite : MonoBehaviour
{
    [SerializeField] CustomizingScriptable scriptable;
    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();    
    }

    public void SetBtnSprite(int clothIndex, int characterIndex, int itemIndex)
    {
        if (!image) return;

        if (clothIndex == 0) //헤어 아이템 배치
        {
            if (characterIndex == 0) image.sprite = scriptable.Chanhee_Sprite_H[itemIndex];
            else if (characterIndex == 1) image.sprite = scriptable.Gayeong_Sprite_H[itemIndex];
            else if (characterIndex == 2) image.sprite = scriptable.Haejin_Sprite_H[itemIndex];
            else if (characterIndex == 3) image.sprite = scriptable.Hana_Sprite_H[itemIndex];
        }
        else if (clothIndex == 1) //상의 아이템 배치
        {
            if (characterIndex == 0) image.sprite = scriptable.Chanhee_Sprite_B[itemIndex];
            else if (characterIndex == 1) image.sprite = scriptable.Gayeong_Sprite_B[itemIndex];
            else if (characterIndex == 2) image.sprite = scriptable.Haejin_Sprite_B[itemIndex];
            else if (characterIndex == 3) image.sprite = scriptable.Hana_Sprite_B[itemIndex];
        }
        else if (clothIndex == 2) //상의 아이템 배치
        {
            if (characterIndex == 0) image.sprite = scriptable.Chanhee_Sprite_P[itemIndex];
            else if (characterIndex == 1) image.sprite = scriptable.Gayeong_Sprite_P[itemIndex];
            else if (characterIndex == 2) image.sprite = scriptable.Haejin_Sprite_P[itemIndex];
            else if (characterIndex == 3) image.sprite = scriptable.Hana_Sprite_P[itemIndex];
        }
        else if (clothIndex == 3) //하의 아이템 배치
        {
            if (characterIndex == 0) image.sprite = scriptable.Chanhee_Sprite_F[itemIndex];
            else if (characterIndex == 1) image.sprite = scriptable.Gayeong_Sprite_F[itemIndex];
            else if (characterIndex == 2) image.sprite = scriptable.Haejin_Sprite_F[itemIndex];
            else if (characterIndex == 3) image.sprite = scriptable.Hana_Sprite_F[itemIndex];
        }
        else if (clothIndex == 4)  //액세서리 아이템 배치
        {
            if (characterIndex == 0) image.sprite = scriptable.Chanhee_Sprite_A[itemIndex];
            else if (characterIndex == 1) image.sprite = scriptable.Gayeong_Sprite_A[itemIndex];
            else if (characterIndex == 2) image.sprite = scriptable.Haejin_Sprite_A[itemIndex];
            else if (characterIndex == 3) image.sprite = scriptable.Hana_Sprite_A[itemIndex];
        }        
    }
}
