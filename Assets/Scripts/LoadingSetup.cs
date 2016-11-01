using UnityEngine;
using System.Collections;

public class LoadingSetup : MonoBehaviour {

    public GameObject[] DisableOnStart;
    public GameObject[] EnableOnStart;

	// Use this for initialization
	void Start () {
	    for(int i = 0; i < DisableOnStart.Length; i++)
        {
            DisableOnStart[i].SetActive(false);
        }
        for (int i = 0; i < EnableOnStart.Length; i++)
        {
            DisableOnStart[i].SetActive(true);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
