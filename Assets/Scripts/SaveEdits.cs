using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// for saving text and having it all loaded on start.
/// </summary>
public class SaveEdits : MonoBehaviour {

    public InputField[] SaveObjs;

	// Use this for initialization
	void Start () {
        Load();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Save()
    {
        int i = 0;
        CBUG.Log("Saving PlayerPrefs!");
        foreach(InputField obj in SaveObjs) {
            CBUG.Log("Key: " + i + " String: " + SaveObjs[i].text);
            PlayerPrefs.SetString(""+i, SaveObjs[i].text);
            i++;
        }
        PlayerPrefs.Save();
    }

    public void Load()
    {
        int i = 0;
        string temp;
        CBUG.Log("Loading PlayerPrefs");
        foreach(InputField obj in SaveObjs) {
            temp = PlayerPrefs.GetString("" + i, "");
            CBUG.Log("Key: " + i + " String: " + temp);
            SaveObjs[i].text = temp;
            i++;
        }
    }
}
