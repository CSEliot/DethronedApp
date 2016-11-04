using UnityEngine;
using System.Collections;

public class CardMan : MonoBehaviour {

    public GameObject[] Cards;

    public static GameObject[] _Cards;

    private static GameObject currentCard;
    private static int currentNum;
    private static int MAX_CARDS;

    private bool fixedFirst = false;

	// Use this for initialization
	void Start () {
        transform.tag = "CardMan";
        MAX_CARDS = Cards.Length;
        _Cards = Cards;
        currentNum = 0;
        currentCard = _Cards[currentNum];
    }
	
	// Update is called once per frame
	void Update () {
        //CBUG.Do("CurrentNum: " + currentNum);

	}

    /// <summary>
    /// Goes from MAX_CARDS -> 0
    /// </summary>
    public static void RoleUp (){
        CBUG.Do("Activated Up!");
        if (currentNum - 1 < 0) //==1 because we never move up last card
            return;
        currentNum--;
        currentCard = _Cards[currentNum];
        currentCard.GetComponent<Animator>().SetBool("IsIn", false);
    }

    /// <summary>
    /// Goes from 0 -> MAX_CARDS
    /// </summary>
    public static void RoleDown()
    {
        CBUG.Do("Activated Down!");
        if (currentNum + 1 > _Cards.Length)
            return;
        currentCard.GetComponent<Animator>().SetBool("IsIn", true);
        currentNum++;
        currentCard = _Cards[currentNum >= _Cards.Length ? _Cards.Length - 1 : currentNum];
    }

    public static CardMan GetRef()
    {
        return GameObject.FindGameObjectWithTag("CardMan").GetComponent<CardMan>();
    }
}
