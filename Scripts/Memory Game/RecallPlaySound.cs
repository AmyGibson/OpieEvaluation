using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallPlaySound : MonoBehaviour {

    private InstructionSound instr_sound;
    private AudioSource _sound_source;
    public AudioSource SoundSource { get { return _sound_source; } }

    // Use this for initialization
    void Start () {
        //get the game object that has all the instruction audio clips
        instr_sound = GameObject.Find("InstructionSound").GetComponent<InstructionSound>();
        _sound_source = GetComponent<AudioSource>();

        PlayInstructionSound();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void PlayInstructionSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.RECALL_INSTRUCTION));
    }

    public void PlayQuitSound()
    {
        _sound_source.Stop();
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.THANKS));
        //StartCoroutine(PlayQuitSoundCo());
    }
    /*
    IEnumerator PlayQuitSoundCo()
    {
        _sound_source.Stop();
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip("thankyou"));
        yield return new WaitWhile(() => _sound_source.isPlaying);
        UnityEngine.SceneManagement.SceneManager.LoadScene("profiles_scene");
    }*/

    public void PlayCardMatchSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.CARD_MATCH));
    }

    public void PlayCardMismatchSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.TRY_AGAIN));
    }


    public void PlayCardSound(AudioClip ac)
    {
        _sound_source.Stop();
        _sound_source.PlayOneShot(ac);
        //StartCoroutine(PlayCardSoundCo(ac));
    }

    /*
    private IEnumerator PlayCardSoundCo(AudioClip ac)
    {
        yield return new WaitWhile(() => _sound_source.isPlaying);
        _sound_source.PlayOneShot(ac);
    }*/



}
