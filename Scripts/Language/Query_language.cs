using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Query_language : MonoBehaviour {
	
	private GameInfo gameInfo;
    private GameDataScript gameData;

    // Use this for initialization
    void Start () {
       
#if UNITY_ANDROID
        Opie.instance().head().set_eye_type(EyeType.NEUTRAL, Opie.Head.instant_action());
        Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f, Opie.Head.transition_action());
#endif
        //When entering this scene, stop the idle motion so that Opie stops scanning 
        //the environment with its eyes
//#if UNITY_ANDROID
//        Opie.instance().behaviours ("idle_motion").stop (Opie.Behaviour.instant_action ());
//#endif
        //Find the object containing profile information
        gameInfo = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        gameData = GameObject.Find("GameData").GetComponent<GameDataScript>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
	
	//This function is called when a language button is pressed
	public void Language_selected(string language){
        gameInfo.Language = language;
        gameInfo.LoadResources();
        gameData.setOrdering(gameInfo.Images.Length);

        UnityEngine.SceneManagement.SceneManager.LoadScene("level_selection");
    }
	
	public void Quit_app(){
        Application.Quit();
    }

}
