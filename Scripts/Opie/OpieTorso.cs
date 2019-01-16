using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;

public class OpieTorso : MonoBehaviour
{
	//public static WifiAndroidManager wifimanager;
	
    static OpieTorso()
    {
		
		
		//Opie.add_game (new PhonemeGame.PhonemeGameInterface ());
       // Opie.add_game(new LookAroundYou.LookAroundYouGameInterface());
	   //Opie.add_game(new Assets.Games.DreamtimeGame.Scripts.OpieGameInterface());
        //Opie.add_game(new AnimalGameControl.OpieGameInterface());
        //Opie.add_game(new Assets.Games.TommyGame.Scripts.OpieGameInterface());
		
		//This code defines which game to launch once the Opie ROS interface is initialized. This will switch to the profile scene when initialized
		Opie.add_game(new Assets.Games.MemoryGame.Scripts.OpieGameInterface());


#if UNITY_ANDROID && !UNITY_EDITOR
        // do not add TTS unless platform is Android
        Debug.Log("Starting TTS");
        Opie.add_driver(OpieTTSDriver.instance());
#endif

        Opie.add_driver(new OpieAudio.OpieAudioInterface());

		//Add Opie idle motion behaviour
		Opie.add_behaviour (new Opie.Behaviour ("idle_motion"));

		//Setting node name (for ROS) and topic name
		Opie.instance ().set_node_name ("opie_2_torso_amy");
		Opie.instance ().set_topic_root ("/opie");
    }

    // Use this for initialization
    void Start ()
	{
        
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (!Opie.instance().update())
        {
            Debug.Log("ERROR: Opie failed to update\n");
        }
    }

	
}

