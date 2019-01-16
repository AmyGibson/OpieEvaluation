using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnexionSettings : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void reattempt_connexion(){
		//This function is called by the "retry connection button", and goes back to the Web Connexion Scene to reattempt connection
		UnityEngine.SceneManagement.SceneManager.LoadScene("WebConnexionScene");
	}
	
public void quit_app(){
	Application.Quit();
}
}
