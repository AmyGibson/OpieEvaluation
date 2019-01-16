using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq;
using UnityEngine.UI;

public class SceneControllerRecall : MonoBehaviour
{
    private int no_of_card;
	private const float offsetX = 0.1f;
    private const float offsetY = 0.1f;


    //public Transform mcbn;
	public Text Transcription_text;

	private GameDataScript gameData;
    private GameInfo gameInfo;
    private RecallResults recallResults;
    private Button quitButton;
    private int question_no = 0;
	
    private RecallPlaySound rps;
	private MemoryCardRecall originalCard;
	//private Star originalStar;
	private Sprite[] images;
	private AudioClip[] sounds;
	private TextAsset[] words;
    
	private AudioClip target_sound = null;
	private string target_transcription;

	private int[] questions;
    private int[] answers;
	private int[] ordering;
    //private List<Star> stars;
    private List<MemoryCardRecall> cards;

    private bool isplaying = false;
    private bool quitPressed = false;

    private static System.Random rng = new System.Random();

	void Start ()
	{
#if UNITY_ANDROID
        Opie.instance().head().set_eye_type(EyeType.NEUTRAL, Opie.Head.instant_action());
        Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f, Opie.Head.transition_action());
#endif

        //Find all the necessary the Game Objects
        gameData = GameObject.Find("GameData").GetComponent<GameDataScript>();
        gameInfo = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        recallResults = GameObject.Find("RecallResults").GetComponent<RecallResults>();
        recallResults.Reset();

        originalCard = GameObject.Find("Memory Card").GetComponent<MemoryCardRecall>();
        //originalStar = GameObject.Find("Star").GetComponent<Star>();
        
        rps = GameObject.Find("RecallPlaySound").GetComponent<RecallPlaySound>();
        quitButton = GameObject.Find("QuitButton").GetComponent<Button>();

        //Retrieve ordering of the images, all the resources should have already been loaded
        ordering = gameData.Ordering;

        images = gameInfo.Images;
        sounds = gameInfo.Sounds;
        words = gameInfo.Words;
        no_of_card = gameData.N_rows * gameData.N_cols / 2;

        //Shuffle the order of the cards to be Question
        questions = Enumerable.Range(0, no_of_card).ToArray();
        
        Shuffle(questions);

        answers = new int[no_of_card];
        for (int i = 0; i < no_of_card; i++)
        {
            answers[i] = ordering[questions[i]];
        }

        CardsSetup();

        DeactivateAllCards();
        //Present the first word (sound + transcription) 
        NextQuestion();
    }


    private void CardsSetup() {

        // positioning
        Vector3 startPos = originalCard.transform.position;
        //Vector3 startPosStar = originalStar.transform.position;
        //stars = new List<Star>();
        cards = new List<MemoryCardRecall>();
        
        //randomly laying out the card
        int[] card_layout_order = Enumerable.Range(0, no_of_card).ToArray();
        Shuffle(card_layout_order);

        // Populating the grid by instantiating the Cards and the Stars (changing color to reflect player performance)
        for (int i = 0; i < no_of_card; i++)
        {
            MemoryCardRecall card;
            //Star star;
            if (i == 0)
            {
                card = originalCard;
                //star = originalStar;
            }
            else
            {
                card = Instantiate(originalCard) as MemoryCardRecall;
                //star = Instantiate(originalStar) as Star;
            }
            //stars.Add(star);
            cards.Add(card);
            int id = answers[card_layout_order[i]];
            card.SetCard(id, images[id]);

           
            int j = (int)Math.Truncate(i / 5f);
            float posX = ((offsetX + card.GetComponent<BoxCollider2D>().bounds.size.x) * (i - j * 5)) + startPos.x;
            float posY = -((offsetY + card.GetComponent<BoxCollider2D>().bounds.size.y) * j) + startPos.y;
            card.transform.position = new Vector3(posX, posY, startPos.z);
           // float posXstar = ((offsetX + star.GetComponent<BoxCollider2D>().bounds.size.y) * i) + startPosStar.x;
           // star.transform.position = new Vector3(posXstar, startPosStar.y, startPosStar.z);
            
        }
    }


    private bool CheckIfDone()
    {

        //If all the cards have been presented
        if (question_no == no_of_card - 1)
        {
            /*
            //Log the performance
            StreamWriter w;
            w = new StreamWriter(Application.persistentDataPath + "/" + log_info.GetDirectoryName()
                + '/' + log_info.GetFileName(), true);
            w.WriteLine("level " + no_of_card.ToString() + " : scored " + Correct_score.ToString() + "\n");
            w.WriteLine("correct : " + string.Join(",", correct.ToArray()) + "\n");
            w.WriteLine("incorrect : " + string.Join(",", incorrect.ToArray()) + "\r\n");
            w.Close();
            */
            //Trigger the level to increase
            //gameData.level_passed();
            //Goes back to Progress Menu
            go_to_next();
            return true;
        }
        return false;
    }

    void Update() {
		
		//React to user touches
		foreach (Vector2 pos in getTouchOrMouseInteractions())
		{
			// Get the origin of the tap
			Vector2 origin = Camera.main.ScreenToWorldPoint(pos);
			float Xeye = 0.5f + origin.x / (22.5f - 2f * origin.y);

            //Get Opie to look where the touch happened
#if UNITY_ANDROID
		    Opie.instance().head().set_linked_pose_and_eye_position(Xeye, 0.0f, Opie.Head.transition_action());
#endif
		}
	}


	//Shuffling array
    // this is public static should this be in a helper class?
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


	//Check for matches between the card selected and the target sound
	public void StartCheckMatch(int id_clicked)
	{
        // deactive all card for these actions to happen (otherwise user can just click through the cards)
        DeactivateAllCards();
        StartCoroutine (CheckMatch(id_clicked));
	}

    public void DeactivateAllCards() {
        for (int i = 0; i < no_of_card; i++)
            cards[i].Deactive();
    }

    private void ActivateAllCards()
    {
        for (int i = 0; i < no_of_card; i++)
            cards[i].Activate();
    }

    private IEnumerator CheckMatch(int id_clicked) {

        // below is to control the game flow
        // if at soon as the game is loaded, someone press an image
        // it will 1. cut off the instruction 2. still play the current word 
        // before going to the next word
        if (question_no == 0)
        {
            rps.SoundSource.Stop(); //to cut off the instruction

            yield return new WaitWhile(() => rps.SoundSource.isPlaying);
            // for some reason it doesnt stop immediately

            yield return new WaitWhile(() => rps.SoundSource.isPlaying);
            // this is to make sure the target word is played in full
        }


        //If there is a match 
        if (id_clicked == answers[question_no]) {

            //Opie is HAPPY if the guess is right
            //StartCoroutine(transientEmotion(EyeType.ATTENTIVE));
			//Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f,Opie.Head.transition_action());

            //Add the trassncripton of the word to the list of correctly recognized words (for teacher feedback)
            recallResults.Correct.Add(target_transcription);
        }
		else {
			//Opie is SAD if the guess is wrong
			//StartCoroutine(transientEmotion(EyeType.SAD));
			//Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f,Opie.Head.transition_action());
            
            //Add the transcirption of the word to the list of incorrectly recognized words (for teacher feedback)
            recallResults.Incorrect.Add(target_transcription);
		}

        if (!CheckIfDone())
        {
            //Increment the question number
            question_no++;
            NextQuestion();
        }
    }


    // block all card clicking when playing a word
    private IEnumerator BlockPlayWord(int current_question_no) {

        // in case some other sound is playing
        yield return new WaitWhile(() => rps.SoundSource.isPlaying);

        PlayTargetSound();
        target_transcription = words[ordering[questions[question_no]]].text;
        Transcription_text.text = target_transcription;
        yield return new WaitWhile(() => rps.SoundSource.isPlaying);

        // the first question by default all cards are activated, and all card are deactivated on clicked
        // but this coroutine is still running (first run at start) so it will activate the card
        // once the first word has been played even the answer has already been clicked
        // and easy way to solve is to not to reactivate the card here at first question
        //if (current_question_no > 0)
            ActivateAllCards();
    }


    private void NextQuestion()
    {
        target_sound = sounds[ordering[questions[question_no]]];

        // the problem is there are multiple block play word running
        // another one is launch before finish check match
        StartCoroutine(BlockPlayWord(question_no));
        
        
        
    }

	//Collect user touches for Opie to trace
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
	
	private IEnumerator transientEmotion(EyeType emote){
		Opie.instance().head().set_eye_type(emote,Opie.Head.instant_action());
		yield return new WaitForSeconds(2);
		Opie.instance().head().set_eye_type(EyeType.NEUTRAL,Opie.Head.instant_action());
	}

	
    private void go_to_next()
    {
        // if the quit button is pressed and is currently playing the quit sound
        // should not progress
        if (!quitPressed)
        {
            quitPressed = true;
            // as soon as the progression has started, can not quit
            //quitButton.interactable = false;
            StartCoroutine(GoToNextCo());
        }
    }

    // if the extra word repetition section is not needed, change scene progression here
    private IEnumerator GoToNextCo()
    {
        // need to play the last card match/mismatch sound
        yield return new WaitWhile(() => rps.SoundSource.isPlaying);
        UnityEngine.SceneManagement.SceneManager.LoadScene("RecallFinish");
    }



    public void PlayTargetSound(){
        rps.PlayCardSound(target_sound);
        
    }
	
	public void quit_app(){

        // the only way for quitPressed to be true here is when the game has started progressing to
        // next stage, so should not quit. this is an extra safeguard in case the disabling the button 
        // doesnt happen fast enough. Unity doesnt always update the UI immediately
        if (quitPressed)
            return;
        DeactivateAllCards();
        quitPressed = true;

        //Opie.instance().head().set_linked_pose_and_eye_position(0.5f, 0.5f, Opie.Head.transition_action());
        gameData.Reset();
        //log_info.Reset();
        //ext_res.Reset();
        
        quitButton.interactable = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
        //StartCoroutine(QuitAppCo());
    }

    /*
    private IEnumerator QuitAppCo()
    {
        rps.PlayQuitSound();
        yield return new WaitWhile(() => rps.SoundSource.isPlaying);
        UnityEngine.SceneManagement.SceneManager.LoadScene("profiles_scene");
    }*/



}
