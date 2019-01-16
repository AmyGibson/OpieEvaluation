using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallResults : MonoBehaviour {
    public static RecallResults instance;
    public List<string> Correct { get; set; }
    public List<string> Incorrect { get; set; }

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

    public void Reset()
    {
        Correct = new List<string>();
        Incorrect = new List<string>();
    }


}
