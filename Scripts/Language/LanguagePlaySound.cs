using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// An object just for sound in this scene

public class LanguagePlaySound : MonoBehaviour
{

    private InstructionSound instr_sound;
    private AudioSource sound_source;

    // Use this for initialization
    void Start()
    {
        //get the game object that has all the instruction audio clips
        instr_sound = GameObject.Find("InstructionSound").GetComponent<InstructionSound>();
        sound_source = GetComponent<AudioSource>();

        //change the audio source to here
        PlayInstructionSound();
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void PlayInstructionSound()
    {
        StartCoroutine(PlayGreetingSound());
    }

    IEnumerator PlayGreetingSound()
    {

        sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.WHATS_LANGUAGE));
        yield return new WaitWhile(() => sound_source.isPlaying);        
    }
}
