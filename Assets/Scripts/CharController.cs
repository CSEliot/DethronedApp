using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CharController : MonoBehaviour, IDragHandler, IPointerClickHandler {

    public enum State
    {
        WaitingOut,
        WaitingIn,
        Moving
    }

    private State PreviousState;
    public State CurrentState;
    public State NextState;

    public float GoToSpeed;
    public float BaseSpeed;
    public float NearbyBuffer;

    public Animator Anim;

    [System.Serializable]
    public struct Stats
    {
        public int ID;
        public string Name;
        public bool needsRoyalty;
        public int[] needsFriends; //SAGE IS OR NOT AND
        public bool isAssailant;
        public bool isCommoner;
        public bool needsCommoner;
    }
    public Stats MyStats;

    private Vector3 toPos;
    private CharSelectionManager charMan;

    private bool notifyOn = false;
    public GameObject NotifyObj;

    // Use this for initialization
    void Start () {
        charMan = GameObject.FindGameObjectWithTag("CharMan").GetComponent<CharSelectionManager>();
        toPos = charMan.GetHomePosStart(MyStats.ID);
        charMan.GetStats(ref MyStats);
        CurrentState = State.WaitingOut;
        NextState = State.WaitingOut;
    }
	
	// Update is called once per frame
	void Update () {
        if((toPos - transform.position).sqrMagnitude > NearbyBuffer &&
            CurrentState != State.Moving)
        {
            GoTo(toPos, CurrentState);
        }
        charMan.ActionBufferAdd.Clear();
        charMan.ActionBufferRemove.Clear();
	}

    public void GoTo(Vector3 _toPos, State newState)
    {
        toPos = _toPos;
        CurrentState = State.Moving;
        NextState = newState;

        Anim.SetTrigger("Walk");
        StartCoroutine(goToHelper());
    }

    public void OnDrag( PointerEventData eventData)
    {
        //Borrowed from Unity DragMe UI Sample
        var rt = GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
        }
    }

    public void SetNotify(bool isOn)
    {
        notifyOn = isOn;
        NotifyObj.SetActive(notifyOn);
        if(notifyOn)
            CBUG.Log("NOTICE ME!");
    }

    public void OnPointerClick( PointerEventData eventData)
    {
        CBUG.Log(MyStats.Name + " was clicked!");
        if (!charMan.includeDependencies(MyStats.ID, CurrentState, NextState))
            return;

        charMan.removeDependencies(MyStats.ID, CurrentState, NextState);


        if (CurrentState == State.Moving)
        {
            if(NextState == State.WaitingIn)
            {
                toPos = charMan.GetHomePos(MyStats.ID);
                NextState = State.WaitingOut;
            }
            else if (NextState == State.WaitingOut && charMan.IsPositionAvailable(1))
            {
                toPos = charMan.GetNextSelectPos(MyStats.ID);
                NextState = State.WaitingIn;
            }
        }else
        {
            if (CurrentState == State.WaitingOut && charMan.IsPositionAvailable(1))
            {
                GoTo(charMan.GetNextSelectPos(MyStats.ID), State.WaitingIn);
            }
            else if (CurrentState == State.WaitingIn)
            {
                GoTo(charMan.GetHomePos(MyStats.ID), State.WaitingOut);
            }
        }
        charMan.SetNotifications();
    }

    private IEnumerator goToHelper()
    {
        float startTime = Time.time;
        float startPos = transform.position.sqrMagnitude;
        CBUG.Log("MOVING!");
        bool idling = false;
        while((toPos - transform.position).sqrMagnitude > NearbyBuffer)
        {
            if((toPos - transform.position).sqrMagnitude < NearbyBuffer * 2f
                && !idling) {
                idling = true;
                Anim.SetTrigger("Idle0");
                //Too much hopping. So let's idle a little bit before the buffer
                // cuts us off.
                CBUG.Log("Idling!");
            }
            transform.Translate((toPos - transform.position) * GoToSpeed * 
                Smooth(transform.position.sqrMagnitude, startPos, toPos.sqrMagnitude, true));
            yield return null;
        }
        CurrentState = NextState;
        StartCoroutine(DesyncAnim("Idle" + Random.Range(1, 3)));
    }

    /// <summary>
    /// returns a smooth value given 0<x<1 
    /// </summary>
    /// <param name="x"> Value between 0 and 1</param>
    /// <returns></returns> 
    private float Smooth(float x)
    {
        if (x < 0f)
            return 1f; //Max Speed
        if (x > 1f)
            return 0f; // MOre than arrived, no move
        return -1f * (x*x*x*x) + 1;
    }

    /// <summary>
    /// Returns a smooth value using -x^4 + 1
    /// </summary>
    /// <param name="_x">Current X</param>
    /// <param name="min">Min X Value [inc]</param>
    /// <param name="max">Max X Value [inc]</param>
    /// <param name="swappable">Means if min > max, swap</param>
    /// <returns></returns>
    private float Smooth(float _x, float min, float max, bool swappable)
    {
        if (max < min && swappable)
        {
            float temp = max;
            max = min;
            min = temp;
        }else if (!swappable)
        {
            CBUG.Error("MIN GREATER THAN MAX");
        }

        if (_x < min)
            return 1f; //Max speed
        if (_x > max)
            return 1f; //More than arrived, no move.
        float x = (_x - min) / (max - min);
        return -1f * (x*x*x*x) + 1 + BaseSpeed;
    }

    private IEnumerator DesyncAnim(string trigger)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        Anim.SetTrigger(trigger);
    }
}
