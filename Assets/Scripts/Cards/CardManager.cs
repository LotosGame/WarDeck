using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private GameObject cardPrefab;

    [Header("Deck Configuration")]
    [SerializeField] private List<CardData> deck = new List<CardData>();

    private List<CardData> currentHand = new List<CardData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Для теста при старте сцены спавним начальные карты
        SpawnInitialHand();
    }

    public void SpawnInitialHand()
    {
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (CardData cardData in deck)
        {
            CreateCardUI(cardData);
        }
    }

    public void CreateCardUI(CardData cardData)
    {
        if (cardPrefab == null || handPanel == null) return;

        GameObject cardObj = Instantiate(cardPrefab, handPanel);
        CardUI cardUI = cardObj.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.Setup(cardData);
        }
    }

    public void OnCardSelected(CardUI cardUI)
    {
        Debug.Log($"Карта выбрана: {cardUI.CardData?.cardName}");
        // Здесь логика разыгрывания карты или спавна юнита на сетку
    }
}