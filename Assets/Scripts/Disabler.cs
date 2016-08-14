using UnityEngine;
using System.Collections;

public class Disabler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DisableNow()
    {
        gameObject.SetActive(false);
    }

    public void DisableParent()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
