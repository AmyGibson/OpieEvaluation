using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


// amy's code to contain all the instruction sound clip and should be accessible for all parts of the game
// these sounds were originally manually linked in the Unity project file
// however there were cases that the linkages of these file were lost during import
// hence the decision of using code to link these sound clips
// which also allow an easier way to change the instruction language if needed (just make sure filenames are the same)
// if new instruction is used one can just change the instruction 

public class InstructionSound : MonoBehaviour {

    public static InstructionSound instance;
    private string instruction_language;
    private AudioClip[] instructionClips = null;
    public const int HELLO = 0;
    public const int WHATS_NAME = 1;
    public const int WHATS_LANGUAGE = 2;
    public const int WHATS_ACTIVITY = 3;
    public const int THANKS = 4;
    public const int GO_LONG = 5;
    public const int MEMEORY_INSTRUCTION = 6;
    public const int RECALL_INSTRUCTION = 7;
    public const int REPEAT = 8;
    public const int CARD_MATCH = 9;
    public const int TRY_AGAIN = 10;
    public const int GO_SHORT = 11;
    private int total_clip_no = 12;



    // initialise the audion clip so that other game objects that call these sounds on start will have them
    void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // if any sound file name changes, they are all listed here and only need to change the below

        instruction_language = "Instructions\\Kriol";

        instructionClips = new AudioClip[total_clip_no];
        instructionClips[HELLO] = Resources.Load<AudioClip>(instruction_language + "_hello");
        instructionClips[WHATS_NAME] = Resources.Load<AudioClip>(instruction_language + "_Wanim_name");
        instructionClips[WHATS_LANGUAGE] = Resources.Load<AudioClip>(instruction_language + "_wanim_langus");
        instructionClips[WHATS_ACTIVITY] = Resources.Load<AudioClip>(instruction_language + "_Start_game");
        instructionClips[THANKS] = Resources.Load<AudioClip>(instruction_language + "_Thank_you_bye");
        instructionClips[GO_LONG] = Resources.Load<AudioClip>(instruction_language + "_press_go_long");
        instructionClips[MEMEORY_INSTRUCTION] = Resources.Load<AudioClip>(instruction_language + "_instructions_memory");
        instructionClips[RECALL_INSTRUCTION] = Resources.Load<AudioClip>(instruction_language + "_instructions_recall");
        instructionClips[CARD_MATCH] = Resources.Load<AudioClip>(instruction_language + "_Gudwan");
        instructionClips[TRY_AGAIN] = Resources.Load<AudioClip>(instruction_language + "_try_again");
        instructionClips[REPEAT] = Resources.Load<AudioClip>(instruction_language + "_repeat");
    }


    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // valid selections are: hello, whatsyourname
    public AudioClip GetInstructionAudioClip(int soundtype)
    {
        return instructionClips[soundtype];
    }

}
