using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class CharSelectionManager : MonoBehaviour {

    public Sprite[] CharSprites;
    public GameObject RedDeckPos;
    public GameObject BlueDeckPos;
    public GameObject SelectedPos;
    public GameObject CharPrefab;
    public int TotalCharacters;
    public Transform SpawnPos;

    public GameObject[] Characters;

    private int[,] suggestionMatrixRed = new int[8, 8];
    private int[,] suggestionMatrixBlue = new int[8, 8];
    public Dictionary<int, int[]> DependenciesOf;
    public Dictionary<int, int[]> NeededBy;
    public Dictionary<int, bool> NeedsRoyalty;
    public Dictionary<int, bool> NeedsCommoner;
    public Dictionary<int, bool> IsAssailant;
    public Dictionary<int, bool> IsCommoner;
    public Dictionary<int, string> ID_to_Name;
    public Dictionary<int, int> ActionBufferAdd; //Callee, Caller
    public Dictionary<int, int> ActionBufferRemove; //Callee, Caller

    private bool[] availablePos;
    private int[] availablePos_ID; //Parallel to availablePos
    private int totalSelected;
    private float totalAssailants;
    private float totalCommoners;
    private float totalRoyalty;
    private int needsRoyaltyIsIn;
    private int needsCommonerIsIn;

    // Use this for initialization
    void Start () {

        buildCharData();
        ActionBufferAdd = new Dictionary<int, int>();
        ActionBufferRemove = new Dictionary<int, int>();
        availablePos_ID = new int[8];
        availablePos = new bool[8];
        for(int x = 0; x < availablePos.Length; x++)
        {
            availablePos[x] = true;
        }

        Characters = new GameObject[TotalCharacters];

        for(int i = 0; i < TotalCharacters; i++)
        {
            Characters[i] = Instantiate(CharPrefab, SpawnPos, false) as GameObject;
            Characters[i].transform.GetChild(0).GetComponent<Image>().sprite = CharSprites[i];
            Characters[i].transform.GetComponent<CharController>().MyStats.ID = i;
        }

        totalSelected = 0;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetNotifications()
    {

        bool testAssailant = totalAssailants < (totalSelected / 2);
        bool testCommoner = totalCommoners < (totalSelected / 2) && needsCommonerIsIn > 0;
        bool testRoyalty = totalRoyalty < (totalSelected / 2) && needsRoyaltyIsIn > 0;


        for(int x = 0; x < Characters.Length; x++) {
            CharController tempChar = Characters[x].GetComponent<CharController>();
            tempChar.SetNotify(false);

            if (testAssailant && tempChar.MyStats.isAssailant) {
                tempChar.SetNotify(true);
            }
            if (testCommoner && tempChar.MyStats.isCommoner) {
                tempChar.SetNotify(true);
            }
            if (testRoyalty && !tempChar.MyStats.isCommoner) {
                tempChar.SetNotify(true);
            }
            if (tempChar.NextState == CharController.State.WaitingIn)
                tempChar.SetNotify(false);
        }
    }

    public bool IsPositionAvailable(int positionsNeeded)
    {
        return (totalSelected+positionsNeeded) <= 8;
    }

    /// <summary>
    /// Returns next position on the tray of selected characters.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetNextSelectPos(int id)
    {
        totalSelected++;
        if (NeedsRoyalty[id])
            needsRoyaltyIsIn++;
        if (NeedsCommoner[id])
            needsCommonerIsIn++;
        if (IsAssailant[id])
            totalAssailants++;
        if (IsCommoner[id])
            totalCommoners++;
        else
            totalRoyalty++;

        int x = 0;
        for(; x < availablePos.Length; x++)
        {
            if (availablePos[x])
            {
                availablePos[x] = false;
                availablePos_ID[x] = id;
                break;         
            }
        }
        return SelectedPos.transform.GetChild(x).position;
    }

    public Vector3 GetHomePosStart(int id)
    {
        if (id < TotalCharacters / 2)
            return RedDeckPos.transform.GetChild(id).transform.position;
        else
            return BlueDeckPos.transform.GetChild(id - 8).transform.position;
    }

    public Vector3 GetHomePos(int id)
    {
        totalSelected--;
        if (NeedsRoyalty[id])
            needsRoyaltyIsIn--;
        if (NeedsCommoner[id])
            needsCommonerIsIn--;
        if (IsAssailant[id])
            totalAssailants--;
        if (IsCommoner[id])
            totalCommoners--;
        else
            totalRoyalty--;

        for (int x = 0; x < availablePos_ID.Length; x++)
        {
            if(availablePos_ID[x] == id)
            {
                availablePos[x] = true;
                break;
            }
        }

        if (id < TotalCharacters / 2)
            return RedDeckPos.transform.GetChild(id).transform.position;
        else
            return BlueDeckPos.transform.GetChild(id - 8).transform.position;
    }
    
    public void GetStats(ref CharController.Stats newStats)
    {
        newStats.needsFriends = DependenciesOf[newStats.ID];
        newStats.isCommoner = IsCommoner[newStats.ID];
        newStats.isAssailant = IsAssailant[newStats.ID];
        newStats.needsRoyalty = NeedsRoyalty[newStats.ID];
        newStats.needsCommoner = NeedsCommoner[newStats.ID];
        newStats.Name = ID_to_Name[newStats.ID];
    }

    /// <summary>
    /// Makes sure there is space available for Char N Friends.
    /// If so, also tell friend to go as well.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="CurrentState"></param>
    /// <param name="NextState"></param>
    /// <returns></returns>
    public bool includeDependencies(int ID, CharController.State CurrentState, CharController.State NextState)
    {
        //If we're already "WaitingIn" Selection, or have no friends, nothing to do here.
        if (DependenciesOf[ID].Length == 0 || NextState == CharController.State.WaitingIn)
            return true;

        //Is there space for us and our friends? If not, don't act!
        int totalDependenciesNotSelected = 0;
        for (int x = 0; x < DependenciesOf[ID].Length; x++) {
            if (Characters[DependenciesOf[ID][x]].GetComponent<CharController>().NextState == CharController.State.WaitingOut)
                totalDependenciesNotSelected++;
        }
        if (!IsPositionAvailable(totalDependenciesNotSelected + 1))
            return false;

        //Tell friends they got "clicked".
        CBUG.Log(ID_to_Name[ID] + " has " + DependenciesOf[ID].Length + " friends to add");
        for (int i = 0; i < DependenciesOf[ID].Length; i++) {
            //To prevent multiple calls to a Character within a single frame, we check action buffer.
            if (!ActionBufferAdd.ContainsKey(DependenciesOf[ID][i])) {
                //Fixing circular dependency with this:
                //Don't call the one who called me.
                //if Caller of Past == Character I want to Call, don't call
                if((!ActionBufferAdd.ContainsKey(ID)) || ActionBufferAdd[ID] != DependenciesOf[ID][i]) {
                    //Don't call anyone already settled in.
                    if(Characters[DependenciesOf[ID][i]].GetComponent<CharController>().NextState != CharController.State.WaitingIn) {
                        ActionBufferAdd.Add(DependenciesOf[ID][i], ID);
                        Characters[DependenciesOf[ID][i]].GetComponent<CharController>().OnPointerClick(null);
                    }
                }
            }
        }
        return true;
    }


    /// <summary>
    /// If someone depends on a char and that char is removed, remove that someone.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="CurrentState"></param>
    /// <param name="NextState"></param>
    /// <returns></returns>
    public void removeDependencies(int ID, CharController.State CurrentState, CharController.State NextState)
    {
        //If we're alreasdy "WaitingOut"side of Selection, or needed by no one, nothing to do here.
        if (NeededBy[ID].Length == 0 || NextState == CharController.State.WaitingOut)
            return;

        //Tell friends we're leaving.
        CBUG.Log(ID_to_Name[ID] + " has " + NeededBy[ID].Length + " references on to remove");
        for (int i = 0; i < NeededBy[ID].Length; i++) {
            //To prevent multiple calls to a Character within a single frame, we check action buffer.
            if (!ActionBufferRemove.ContainsKey(NeededBy[ID][i])) {
                //Fixing circular dependency with this:
                //Don't call the one who called me.
                //if Caller of Past == Character I want to Call, don't call
                if ((!ActionBufferRemove.ContainsKey(ID)) || ActionBufferRemove[ID] != NeededBy[ID][i]) {
                    if (Characters[NeededBy[ID][i]].GetComponent<CharController>().NextState != CharController.State.WaitingOut) {
                        ActionBufferRemove.Add(NeededBy[ID][i], ID);
                        Characters[NeededBy[ID][i]].GetComponent<CharController>().OnPointerClick(null);
                    }
                }
            }
        }
    }

    //public void UpdateSetupNeeds(int ID, CharController.State )
    //{

    //}

    /// <summary>
    /// Friends are roles that THAT role needs to also be in play.
    /// </summary>
    private void buildCharData()
    {
        NeededBy = new Dictionary<int, int[]>();
        NeededBy.Add(0, new int[3] { 1, 3, 5});
        NeededBy.Add(1, new int[0]);
        NeededBy.Add(2, new int[0]);
        NeededBy.Add(3, new int[2] { 5, 6 });
        NeededBy.Add(4, new int[0]);
        NeededBy.Add(5, new int[1] { 6 });
        NeededBy.Add(6, new int[0]);
        NeededBy.Add(7, new int[0]);
        NeededBy.Add(8, new int[1] { 9 });
        NeededBy.Add(9, new int[1] { 8 });
        NeededBy.Add(10, new int[0]);
        NeededBy.Add(11, new int[0]);
        NeededBy.Add(12, new int[0]);
        NeededBy.Add(13, new int[0]);
        NeededBy.Add(14, new int[0]);
        NeededBy.Add(15, new int[0]);
        DependenciesOf = new Dictionary<int, int[]>();
        DependenciesOf.Add(0, new int[0]);
        DependenciesOf.Add(1, new int[1] { 0 });
        DependenciesOf.Add(2, new int[0]);
        DependenciesOf.Add(3, new int[1] { 0 });
        DependenciesOf.Add(4, new int[0]);
        DependenciesOf.Add(5, new int[2] { 0, 3 });
        DependenciesOf.Add(6, new int[2] { 3, 5 });//SAGE IS AN OR NOT AND
        DependenciesOf.Add(7, new int[0]);
        DependenciesOf.Add(8, new int[1] { 9 });
        DependenciesOf.Add(9, new int[1] { 8 });
        DependenciesOf.Add(10, new int[0]);
        DependenciesOf.Add(11, new int[0]);
        DependenciesOf.Add(12, new int[0]);
        DependenciesOf.Add(13, new int[0]);
        DependenciesOf.Add(14, new int[0]);
        DependenciesOf.Add(15, new int[0]);
        NeedsCommoner = new Dictionary<int, bool>();
        NeedsCommoner.Add(0, false);
        NeedsCommoner.Add(1, false);
        NeedsCommoner.Add(2, false);
        NeedsCommoner.Add(3, false);
        NeedsCommoner.Add(4, false);
        NeedsCommoner.Add(5, false);
        NeedsCommoner.Add(6, false);
        NeedsCommoner.Add(7, true);
        NeedsCommoner.Add(8, false);
        NeedsCommoner.Add(9, false);
        NeedsCommoner.Add(10, false);
        NeedsCommoner.Add(11, false);
        NeedsCommoner.Add(12, false);
        NeedsCommoner.Add(13, false);
        NeedsCommoner.Add(14, false);
        NeedsCommoner.Add(15, false);
        IsCommoner = new Dictionary<int, bool>();
        IsCommoner.Add(0, false);
        IsCommoner.Add(1, false);
        IsCommoner.Add(2, true);
        IsCommoner.Add(3, true);
        IsCommoner.Add(4, true);
        IsCommoner.Add(5, false);
        IsCommoner.Add(6, true);
        IsCommoner.Add(7, true);
        IsCommoner.Add(8, true);
        IsCommoner.Add(9, true);
        IsCommoner.Add(10, false);
        IsCommoner.Add(11, true);
        IsCommoner.Add(12, true);
        IsCommoner.Add(13, true);
        IsCommoner.Add(14, false);
        IsCommoner.Add(15, false);
        IsAssailant = new Dictionary<int, bool>();
        IsAssailant.Add(0, false);
        IsAssailant.Add(1, false);
        IsAssailant.Add(2, true);
        IsAssailant.Add(3, true);
        IsAssailant.Add(4, false);
        IsAssailant.Add(5, true);
        IsAssailant.Add(6, false);
        IsAssailant.Add(7, true);
        IsAssailant.Add(8, false);
        IsAssailant.Add(9, false);
        IsAssailant.Add(10, true);
        IsAssailant.Add(11, true);
        IsAssailant.Add(12, true);
        IsAssailant.Add(13, true);
        IsAssailant.Add(14, false);
        IsAssailant.Add(15, true);
        NeedsRoyalty = new Dictionary<int, bool>();
        NeedsRoyalty.Add(0, false);
        NeedsRoyalty.Add(1, false);
        NeedsRoyalty.Add(2, false);
        NeedsRoyalty.Add(3, false);
        NeedsRoyalty.Add(4, false);
        NeedsRoyalty.Add(5, false);
        NeedsRoyalty.Add(6, false);
        NeedsRoyalty.Add(7, false);
        NeedsRoyalty.Add(8, false);
        NeedsRoyalty.Add(9, false);
        NeedsRoyalty.Add(10, true);
        NeedsRoyalty.Add(11, true);
        NeedsRoyalty.Add(12, false);
        NeedsRoyalty.Add(13, false);
        NeedsRoyalty.Add(14, false);
        NeedsRoyalty.Add(15, false);
        ID_to_Name = new Dictionary<int, string>();
        ID_to_Name.Add(0, "King");
        ID_to_Name.Add(1, "King's Hand");
        ID_to_Name.Add(2, "Assassin");
        ID_to_Name.Add(3, "Fool");
        ID_to_Name.Add(4, "ShopKeeper");
        ID_to_Name.Add(5, "Princess");
        ID_to_Name.Add(6, "Sage");
        ID_to_Name.Add(7, "Maniac");
        ID_to_Name.Add(8, "Suitor");
        ID_to_Name.Add(9, "Sweetheart");
        ID_to_Name.Add(10, "Alchemist");
        ID_to_Name.Add(11, "Anarchist");
        ID_to_Name.Add(12, "Butcher");
        ID_to_Name.Add(13, "Butcher's Boy");
        ID_to_Name.Add(14, "Arbiter");
        ID_to_Name.Add(15, "Sentinel");
    }

}
