using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
	private int gridRows;
	private int gridCols;
	private const float offsetX = 0.1f;
	private const float offsetY = 0.1f;
	private string language;
	public Text Transcription_text;
	
	private GameDataScript gameData;
    private GameInfo gameInfo;


    private MemoryCard _firstRevealed;
	private MemoryCard _secondRevealed;
	private bool _canClick = true;
    private int winning_score;
    private int current_score = 0;
    private MemoryPlaySound mps;
    private Button quitButton;


    private int[] ordering;

    private MemoryCard originalCard;
	private Sprite[] images;
	private AudioClip[] sounds;
    private TextAsset[] words;
    private bool quitPressed = false;
    
   
    State STATE_NONE_SELECTED = new State ("STATE_NONE_SELECTED");
	State STATE_FIRST_FLIPPING = new State ("STATE_FIRST_FLIPPING");
	State STATE_FIRST_PLAYING = new State ("STATE_FIRST_PLAYING");
	State STATE_FIRST_SELECTED = new State ("STATE_FIRST_SELECTED");
	State STATE_SECOND_FLIPPING = new State ("STATE_SECOND_FLIPPING");
	State STATE_SECOND_PLAYING = new State ("STATE_SECOND_PLAYING");
	State STATE_SECOND_SELECTED = new State ("STATE_SECOND_SELECTED");
	State STATE_MATCHING = new State ("STATE_MATCHING");
	State STATE_FIRST_FLIPPING_BACK = new State ("STATE_FIRST_FLIPPING_BACK");
	State STATE_SECOND_FLIPPING_BACK = new State ("STATE_SECOND_FLIPPING_BACK");


	Event EVENT_CLICK = new Event("EVENT_CLICK");
	Event EVENT_FINISHED = new Event ("EVENT_FINISHED");
	Event EVENT_SCORED = new Event ("EVENT_SCORED");

	private MemoryCard current_card = null;

	private static System.Random rng = new System.Random();  

	public bool canReveal {
		get {return _secondRevealed == null;}
	}


	private StateMachine _state_machine;
	public StateMachine state_machine {
		get { return _state_machine; }
		private set { _state_machine = value; }
	}


    private void Awake()
    {
        quitButton = GameObject.Find("QuitButton").GetComponent<Button>();
        gameInfo = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        gameData = GameObject.Find("GameData").GetComponent<GameDataScript>();
        mps = GameObject.Find("MemoryPlaySound").GetComponent<MemoryPlaySound>();
        language = gameInfo.Language;
        //Retrieve ordering of the images
        ordering = gameData.Ordering;

        images = gameInfo.Images;
        sounds = gameInfo.Sounds;
        words = gameInfo.Words; 

    }

    void Start ()
	{

        //Opie looks up with NEUTRAL emotion
#if UNITY_ANDROID
		Opie.instance().head().set_eye_type(EyeType.NEUTRAL,Opie.Head.instant_action());
		Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f,Opie.Head.transition_action());
#endif
        // there should only be these levels
        if (gameInfo.Level == 1) //2 words, 4 tiles
        {
            gameData.arrange_grid(2);
        }else if (gameInfo.Level == 2) //3 words, 6 tiles
        {
            gameData.arrange_grid(3);
        }else if (gameInfo.Level == 3) //4 words, 8 tiles
        {
            gameData.arrange_grid(4);
        }else if (gameInfo.Level == 4) //6 words, 12 tiles
        {
            gameData.arrange_grid(6);
        }



        StateMachineInit();
        CardsSetup();
        

    }		

	
	void Update() {
#if UNITY_ANDROID
		//Define what happens when the user touches
		foreach (Vector2 pos in getTouchOrMouseInteractions())
		{
			// Get the origin of the tap
			Vector2 origin = Camera.main.ScreenToWorldPoint(pos);
			float Xeye = 0.5f + origin.x / (22.5f - 2f * origin.y);

            //Get Opie to look at the position where the touch occurred	

		    Opie.instance().head().set_linked_pose_and_eye_position(Xeye, 0.0f, 
                Opie.Head.transition_action());
    }
#endif
        //For debugging only (keyboard)
        if (Input.GetKey("escape")){
			StopAllCoroutines ();
			Application.Quit ();
		}
				
		
		//Update the state machine
		state_machine.update ();
	}

    private void StateMachineInit()
    {

        //Defining the State machine defining the game states
        state_machine = new StateMachine(
            new Transition[]{
                new Transition{from = STATE_NONE_SELECTED, ev = EVENT_CLICK, to = STATE_FIRST_FLIPPING},
                new Transition{from = STATE_FIRST_FLIPPING, ev = EVENT_FINISHED, to = STATE_FIRST_PLAYING},
                new Transition{from = STATE_FIRST_PLAYING, ev = EVENT_FINISHED, to = STATE_FIRST_SELECTED},
                new Transition{from = STATE_FIRST_SELECTED, ev = EVENT_CLICK, to = STATE_SECOND_FLIPPING},
                new Transition{from = STATE_SECOND_FLIPPING, ev = EVENT_FINISHED, to = STATE_SECOND_PLAYING},
                new Transition{from = STATE_SECOND_PLAYING, ev = EVENT_FINISHED, to = STATE_MATCHING},
                new Transition{from = STATE_MATCHING, ev = EVENT_FINISHED, to = STATE_FIRST_FLIPPING_BACK},
                new Transition{from = STATE_MATCHING, ev = EVENT_SCORED, to = STATE_NONE_SELECTED},
                new Transition{from = STATE_FIRST_FLIPPING_BACK, ev = EVENT_FINISHED, to = STATE_SECOND_FLIPPING_BACK},
                new Transition{from = STATE_SECOND_FLIPPING_BACK, ev = EVENT_FINISHED, to = STATE_NONE_SELECTED}

            }
        );

        state_machine.onTransition = (Transition transition) => {
           // Debug.Log("transition: " + transition.from.name + ": " + transition.ev.name + " -> " + transition.to.name);
        };

        //State reached when the first card has been selected
        STATE_FIRST_FLIPPING.onEnter = (State state, Event ev) => {
            current_card.StartFlipAndPlay();
        };

        //State reached when the second card has been selected
        STATE_SECOND_FLIPPING.onEnter = (State state, Event ev) => {
            current_card.StartFlipAndPlay();
        };

        //Once both cards are selected, check if they match
        STATE_MATCHING.onEnter = (State state, Event ev) => {
            StartCheckMatch();
        };

        //Once the cards are checked, go back to the "default" state with no cards selected.
        STATE_NONE_SELECTED.onEnter = (State state, Event ev) => {
            //Opie looks down at its tummy to invite the user to tap the cards
#if UNITY_ANDROID
            Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.0f, Opie.Head.transition_action());
#endif
        };


        //State machine initialization
        state_machine.init(STATE_NONE_SELECTED);

    }


    private void CardsSetup() {

        originalCard = GameObject.Find("Memory Card").GetComponent<MemoryCard>();

        //Prepare the grid to arrange the cards
        gridRows = gameData.N_rows;
        gridCols = gameData.N_cols;

        Vector3 startPos = originalCard.transform.position;
        int size = gridRows * gridCols;
        if (size % 2 != 0)
        {
            Debug.Log("ERROR: rows * cols must be divisible by 2");
            return;
        }
        //Numbering the pairs (2 cards for each number)
        List<int> numbers = new List<int>();
        for (int i = 0; i < size / 2; i++)
        {
            numbers.Add(i % images.Length);
            numbers.Add(i % images.Length);
        }
        //Setting winning score to the number of pairs
        winning_score = size / 2;

        //Shuffling the cards number
        Shuffle(numbers);
        //Populating the grid by instantiating the original card
        for (int i = 0; i < gridCols; i++)
        {
            for (int j = 0; j < gridRows; j++)
            {
                MemoryCard card;
                if (i == 0 && j == 0)
                {
                    card = originalCard;
                }
                else
                {
                    card = Instantiate(originalCard) as MemoryCard;
                }

                int index = j * gridCols + i;
                int id = numbers[index];
                card.SetCard(id, images[ordering[id]], sounds[ordering[id]], words[ordering[id]].text); //.text

                float posX = ((offsetX + card.GetComponent<BoxCollider2D>().bounds.size.x) * i) + startPos.x;
                float posY = -((offsetY + card.GetComponent<BoxCollider2D>().bounds.size.y) * j) + startPos.y;
                card.transform.position = new Vector3(posX, posY, startPos.z);
            }
        }
    }


	//Shuffling array
	public static void Shuffle<T>(IList<T> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;
			int k = rng.Next(n + 1);  
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}  
	}

	//When a card is revealed, memorize its value (for matching)
	public void CardRevealed(MemoryCard card) {
		if (_firstRevealed == null) {
			_firstRevealed = card;
		} else {
			_secondRevealed = card;
		}
	}
	
	//Displays the transcription of the word
	public void SetTranscription(string transc){
		Transcription_text.text = transc;
	}

	public void StartCheckMatch()
	{
		StartCoroutine (CheckMatch ());
	}

	//Check if the cards match
	private IEnumerator CheckMatch() {
		//If the cards match
		if (_firstRevealed.id == _secondRevealed.id) {
#if UNITY_ANDROID
			StartCoroutine(transientEmotion(EyeType.ATTENTIVE));
			Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f,Opie.Head.transition_action());
#endif
            current_score++;

            // prevent overlapping of sound, so far the only condition is if it is playing the quit sound
            if (!mps.SoundSource.isPlaying)
                mps.PlayCardMatchSound();

			//Debug.Log ("Scored in state " + state_machine.state.name);
			Scored();
			
			//If the score reaches the maximum, then go to next
			if (current_score == winning_score) {
                go_to_next();
                //StartCoroutine(go_to_next());
            }
		}
		else {
            //If the cards don't match, Opie looks sad
#if UNITY_ANDROID
			StartCoroutine(transientEmotion(EyeType.SAD));
			Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f,Opie.Head.transition_action());
#endif
            mps.PlayCardMismatchSound();
			yield return new WaitForSeconds(.5f);
			Finish();

			//If the cards don't match, unreveal them
			_firstRevealed.Unreveal();
			_secondRevealed.Unreveal();
		}
		
		//Reinitialize the variables containing the values of the selected cards
		_firstRevealed = null;
		_secondRevealed = null;


	}

	//Restarts the game
	public void Restart() {		
		SceneManager.LoadScene ("main-scene");
	}

	public void SetCurrentCard(MemoryCard memory_card)
	{
		this.current_card = memory_card;
	}

	//Sends a CLICK event (used for state machine transitions)
	public void Click()
	{
		state_machine.send_event_immediate(EVENT_CLICK);
	}

	//Sends a FINISH event (for state machine transitions)
	public void Finish()
	{
		state_machine.send_event_immediate(EVENT_FINISHED);
	}

	//Sends a SCORED event (for SM transitions)
	public void Scored()
	{
		state_machine.send_event_immediate(EVENT_SCORED);
	}

	//Get touch inputs from user
	IEnumerable<Vector2> getTouchOrMouseInteractions()
	{
		if (Input.GetMouseButton(0))
		{
			yield return Input.mousePosition;
		}
		for (int i = 0; i < Input.touchCount; ++i)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
				// Get the origin of the tap
				yield return Input.GetTouch(i).position;
			}
		}
	}
	
	//Displays an emotion (eye sprites) transiently for 2 seconds, then go back to neutral
	private IEnumerator transientEmotion(EyeType emote){
		Opie.instance().head().set_eye_type(emote,Opie.Head.instant_action());
		yield return new WaitForSeconds(2);
		Opie.instance().head().set_eye_type(EyeType.NEUTRAL,Opie.Head.instant_action());
	}

    //Called when the victory is reached, moves on to the recall stage of the game
    private void go_to_next()
    {
        // if the quit button is pressed and is currently playing the quit sound
        // should not progress
        if (!quitPressed)
        {
            quitPressed = true;
            // as soon as the progression has started, can not quit
           // quitButton.interactable = false; 
            StartCoroutine(GoToNextCo());
        }
    }

    private IEnumerator GoToNextCo(){
        // need to play the last correct card match sound
        yield return new WaitWhile(() => mps.SoundSource.isPlaying);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Recall");
     }

    //If quitting the game at that stge, go back to login screen
    public void quit_app(){

        // the only way for quitPressed to be true here is when the game has started progressing to
        // next stage, so should not quit. this is an extra safeguard in case the disabling the button 
        // doesnt happen fast enough. Unity doesnt always update the UI immediately
        if (quitPressed)
            return;

        quitPressed = true;
#if UNITY_ANDROID
        Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f, Opie.Head.transition_action());
#endif
        gameData.Reset();
        quitButton.interactable = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
        //StartCoroutine(QuitAppCo());
    }
	
	/*
	private IEnumerator QuitAppCo(){
        mps.PlayQuitSound();
        yield return new WaitWhile (()=> mps.SoundSource.isPlaying);
		UnityEngine.SceneManagement.SceneManager.LoadScene("profiles_scene");
	}*/
    
}
