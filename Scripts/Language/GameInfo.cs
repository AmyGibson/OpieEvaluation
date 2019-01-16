using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour {

    // this class contains info that is speciific to the content of this use of this app

    public string Language { get; set; }
    public int Level { get; set; }
    public static GameInfo instance;
    public Sprite[] Images { get; private set; }
    public AudioClip[] Sounds { get; private set; }
    public TextAsset[] Words { get; private set; }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    // Use this for initialization
    void Start () {
        Reset();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public string CurrentLevelToString() {
        if (Level == 1)
            return "Transition";
        if (Level == 2)
            return "Yr 1-2";
        if (Level == 3)
            return "Yr 3-4";
        if (Level == 4)
            return "Yr 5-6";
        return null;

    }

    public void Reset()
    {
        Language = "";
        Level = 0;
        Images = null;
        Sounds = null;
        Words = null;
    }

    public void LoadResources()
    {
        if (Language == "")
        {
            Debug.Log("Language is not set");
            return;
        }

        Images = Resources.LoadAll<Sprite>(Language + "/pictures");
        Sounds = Resources.LoadAll<AudioClip>(Language + "/sounds");
        Words = Resources.LoadAll<TextAsset>(Language + "/words");
    }

}
