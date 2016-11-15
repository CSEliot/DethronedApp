using UnityEngine;
using System.Collections;

public class BookMan : MonoBehaviour {

    public GameObject[] Pages;

    public static GameObject[] _Pages;

    private static GameObject currentPage;
    private static int currentNum;
    private static int MAX_CARDS;

    private bool fixedFirst = false;

    // Use this for initialization
    void Start () {
        transform.tag = "BookMan";
        MAX_CARDS = Pages.Length;
        _Pages = Pages;
        currentNum = 0;
        currentPage = _Pages[currentNum];
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    /// <summary>
    /// Goes from MAX_CARDS -> 0
    /// </summary>
    public static void RoleUp (){
        CBUG.Do("Activated Up!");
        if (currentNum - 1 < 0) //==1 because we never move up last card
            return;
        currentNum--;
        currentPage = _Pages[currentNum];
        currentPage.GetComponent<Animator>().SetBool("IsIn", false);
    }

    /// <summary>
    /// Goes from 0 -> MAX_CARDS
    /// </summary>
    public static void RoleDown()
    {
        CBUG.Do("Activated Down!");
        if (currentNum + 1 > _Pages.Length)
            return;
        currentPage.GetComponent<Animator>().SetBool("IsIn", true);
        currentNum++;
        currentPage = _Pages[currentNum >= _Pages.Length ? _Pages.Length - 1 : currentNum];
    }

    public static BookMan GetRef()
    {
        return GameObject.FindGameObjectWithTag("BookMan").GetComponent<BookMan>();
    }
}
