using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallFinishScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MoveToResult()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Results");
    }
}
