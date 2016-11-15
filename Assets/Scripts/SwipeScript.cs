using UnityEngine;
using System.Collections;

/// <summary>
/// FOUND ONLINE, NOT MINE http://pfonseca.com/swipe-detection-on-unity/
/// Is modified/
/// </summary>
public class SwipeScript : MonoBehaviour
{


    private float fingerStartTime = 0.0f;
    private Vector2 fingerStartPos = Vector2.zero;

    private bool isSwipe = false;
    private float minSwipeDist = 25.0f;
    private float maxSwipeTime = 0.30f;

    public int touches;

    public enum ViewState
    {
        Roles,
        Cards,
        Books
    };
    public ViewState CurrentState = ViewState.Roles;
    
    void Start()
    {
        Input.simulateMouseWithTouches = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == ViewState.Roles)
            return;

        if (Input.GetKeyDown("w")) {
            if (CurrentState == ViewState.Cards)
                CardMan.RoleUp();
            else
                BookMan.RoleUp();
        }

        if (Input.GetKeyDown("s")) {
            if (CurrentState == ViewState.Cards)
                CardMan.RoleDown();
            else
                BookMan.RoleDown();
        }


        touches = Input.touchCount;
        if (touches > 0 || Input.GetMouseButtonDown(0)) {
            Debug.Log("OOOOH");
            foreach (Touch touch in Input.touches) {
                switch (touch.phase) {
                    case TouchPhase.Began:
                        /* this is a new touch */
                        isSwipe = true;
                        fingerStartTime = Time.time;
                        fingerStartPos = touch.position;
                        break;

                    case TouchPhase.Canceled:
                        /* The touch is being canceled */
                        isSwipe = false;
                        break;

                    case TouchPhase.Ended:

                        float gestureTime = Time.time - fingerStartTime;
                        float gestureDist = (touch.position - fingerStartPos).magnitude;

                        if (isSwipe && gestureTime < maxSwipeTime && gestureDist > minSwipeDist) {
                            Vector2 direction = touch.position - fingerStartPos;
                            Vector2 swipeType = Vector2.zero;

                            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                                // the swipe is horizontal:
                                swipeType = Vector2.right * Mathf.Sign(direction.x);
                            } else {
                                // the swipe is vertical:
                                swipeType = Vector2.up * Mathf.Sign(direction.y);
                            }

                            if (swipeType.x != 0.0f) {
                                if (swipeType.x > 0.0f) {
                                    // MOVE RIGHT
                                } else {
                                    // MOVE LEFT
                                }
                            }

                            if (swipeType.y != 0.0f) {
                                if (swipeType.y > 0.0f) {
                                    // MOVE UP
                                    CBUG.Log("VERT SWIPE UP");
                                    if (CurrentState == ViewState.Cards)
                                        CardMan.RoleUp();
                                    else
                                        BookMan.RoleUp();
                                } else {
                                    CBUG.Log("VERT SWIPE Down");
                                    if (CurrentState == ViewState.Cards)
                                        CardMan.RoleDown();
                                    else
                                        BookMan.RoleDown();// MOVE DOWN
                                }
                            }
                        }

                        break;
                }
            }
        }
    }

    public void ChangeState(int NewStateInt)
    {
        ViewState newState = (ViewState)NewStateInt;
        CurrentState = newState;
    }
}