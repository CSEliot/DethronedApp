using UnityEngine;
using System.Collections;

public class Disabler : MonoBehaviour {

    public GameObject EnableOther;
    public bool DisableSelf;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DisableNow()
    {
        if (DisableSelf) {
            gameObject.SetActive(false);
        }
        if (EnableOther != null) {
            EnableOther.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }

    public void DisableParent()
    {
        if (DisableSelf) {
            gameObject.SetActive(false);
        }
        if(EnableOther != null) {
            EnableOther.SetActive(true);
        }else {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
