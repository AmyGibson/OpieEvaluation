using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallFinishPlaySound : MonoBehaviour {

    private InstructionSound instrSound;
    private AudioSource soundSource;

    // Use this for initialization
    void Start()
    {
        //get the game object that has all the instruction audio clips
        instrSound = GameObject.Find("InstructionSound").GetComponent<InstructionSound>();
        soundSource = GetComponent<AudioSource>();

        soundSource.PlayOneShot(instrSound.GetInstructionAudioClip(InstructionSound.THANKS));
    }

    // Update is called once per frame
        void Update () {
		
	}
}
