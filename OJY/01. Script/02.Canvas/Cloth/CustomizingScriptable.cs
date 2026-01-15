using UnityEngine;

[CreateAssetMenu(fileName = "Custom Data", menuName = "Scriptable Object/Custom Data", order = int.MaxValue)]
public class CustomizingScriptable : ScriptableObject
{
    [Header("찬희")]
    public Sprite[] Chanhee_Sprite_H;
    public Sprite[] Chanhee_Sprite_B;
    public Sprite[] Chanhee_Sprite_P;
    public Sprite[] Chanhee_Sprite_F;
    public Sprite[] Chanhee_Sprite_A;

    [Header("가영")]
    public Sprite[] Gayeong_Sprite_H;
    public Sprite[] Gayeong_Sprite_B;
    public Sprite[] Gayeong_Sprite_P;
    public Sprite[] Gayeong_Sprite_F;
    public Sprite[] Gayeong_Sprite_A;

    [Header("해진")]
    public Sprite[] Haejin_Sprite_H;
    public Sprite[] Haejin_Sprite_B;
    public Sprite[] Haejin_Sprite_P;
    public Sprite[] Haejin_Sprite_F;
    public Sprite[] Haejin_Sprite_A;

    [Header("하나")]
    public Sprite[] Hana_Sprite_H;
    public Sprite[] Hana_Sprite_B;
    public Sprite[] Hana_Sprite_P;
    public Sprite[] Hana_Sprite_F;
    public Sprite[] Hana_Sprite_A;
}
