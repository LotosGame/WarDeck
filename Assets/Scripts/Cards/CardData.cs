using UnityEngine;
using UnityEngine.Serialization;

public enum CardType
{
    Unit,
    Building,
    Spell,
    Tactic
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Info")]
    public string cardName = "Warrior";
    [TextArea] public string description;
    public CardType cardType = CardType.Unit;

    [Header("Stats")]
    [FormerlySerializedAs("manaCost")]
    public int cost = 1;

    [Header("Design")]
    [FormerlySerializedAs("icon")]
    [Tooltip("Полный дизайн карточки. Кинь картинку в Assets/Cards/Art и перетащи сюда — она растянется на весь размер карты.")]
    public Sprite cardArt;
    public Color backgroundColor = Color.white;
    public Color iconTint = Color.white;

    [Header("Gameplay")]
    public GameObject unitPrefab;
    [Tooltip("Спрайт юнита/башни на поле боя. Если назначен, юнит получит этот спрайт.")]
    public Sprite unitSprite;
}
