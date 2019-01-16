using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPlaySound : MonoBehaviour {

    private InstructionSound instr_sound;
    private AudioSource _sound_source;
    public AudioSource SoundSource { get { return _sound_source; } }

    // Use this for initialization
    void Start()
    {
        //get the game object that has all the instruction audio clips
        GameObject isgo = GameObject.Find("InstructionSound");
        instr_sound = isgo.GetComponent<InstructionSound>();
        _sound_source = GetComponent<AudioSource>();

        PlayInstructionSound();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PlayInstructionSound() {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.MEMEORY_INSTRUCTION));
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


    // no need to wait for the go introduction to finish before changing scene
    // so no need for coroutin and yield
    public void PlayGoLongSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.GO_LONG));
        //  StartCoroutine(PlayGoLongSoundCo());
    }

    public void PlayCardMatchSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.CARD_MATCH));        
    }

    public void PlayCardMismatchSound()
    {
        _sound_source.PlayOneShot(instr_sound.GetInstructionAudioClip(InstructionSound.TRY_AGAIN));
    }

    
    public void PlayCardSound(AudioClip ac) {
        _sound_source.PlayOneShot(ac);
    }
}
