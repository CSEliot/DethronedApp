using UnityEngine;
using System.Collections;

/// <summary>
/// Simple tool for managing PlayerPrefs in-editor. RECOMMENDED TO HAVE EXECUTION ORDER FIRST.
/// </summary>
public class PlayerPrefsTesting : MonoBehaviour {

    public bool ClearAll;

    public string ClearKey;
    public bool Clear;

	// Use this for initialization
	void Start () {
        if (ClearAll) {
            PlayerPrefs.DeleteAll();
            ClearAll = false;
            CBUG.Log("Clearing All PlayerPrefs!");
        }

        if (Clear) {
            Clear = false;
            PlayerPrefs.DeleteKey(ClearKey);
            CBUG.Log("Clearing Key: " + ClearKey);
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (ClearAll) {
            PlayerPrefs.DeleteAll();
            ClearAll = false;
            CBUG.Log("Clearing All PlayerPrefs!");
        }

        if (Clear) {
            Clear = false;
            PlayerPrefs.DeleteKey(ClearKey);
            CBUG.Log("Clearing Key: " + ClearKey);
        }
	
	}
}
