using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour {

    public Text currentLevel;
    private GameInfo game_info;

	// Use this for initialization
	void Start () {
        //When entering this scene, stop the idle motion so that Opie stops 
        //scanning the environment with its eyes
#if UNITY_ANDROID
        Opie.instance().behaviours("idle_motion").stop(Opie.Behaviour.instant_action());
#endif
        game_info = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        currentLevel.text = game_info.CurrentLevelToString();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NextStudent() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("main-scene");
    }

    public void RechooseLevel() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("level_selection");
    }

    public void QuitApp()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
    }

}
