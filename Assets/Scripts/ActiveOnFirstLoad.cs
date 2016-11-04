using UnityEngine;
using System.Collections;

public class ActiveOnFirstLoad : MonoBehaviour {

    public Component ToDisable;

	// Use this for initialization
	void Start () {
	    if (PlayerPrefs.GetInt("FirstLoad", 0) == 0) {
            PlayerPrefs.SetInt("FirstLoad", 1);
            PlayerPrefs.Save();
        }else {
            (ToDisable as Behaviour).enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
