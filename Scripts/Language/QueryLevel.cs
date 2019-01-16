using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueryLevel : MonoBehaviour {

    private GameInfo game_info;

    // Use this for initialization
    void Start()
    {
        //When entering this scene, stop the idle motion so that Opie stops 
        //scanning the environment with its eyes
#if UNITY_ANDROID
        Opie.instance().behaviours("idle_motion").stop(Opie.Behaviour.instant_action());
#endif
        //Find the object containing profile information
        game_info = GameObject.Find("GameInfo").GetComponent<GameInfo>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //This function is called when a level button is pressed
    public void Level_selected(int level)
    {
        //In logging information, set the language
        game_info.Level = level;
        //Go to the activity selection scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }

    public void Quit_app()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
        //Application.Quit();
    }
}
