using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsController : MonoBehaviour {

    public Text ResultsTextLeft;
    public Text ResultsTextRight;
    private RecallResults recallResults;

    // these are for better ordering of results
    private GameDataScript gameData; // ordering
    private GameInfo gameInfo; // actual words

	// Use this for initialization
	void Start () {
        gameData = GameObject.Find("GameData").GetComponent<GameDataScript>();
        gameInfo = GameObject.Find("GameInfo").GetComponent<GameInfo>();
        recallResults = GameObject.Find("RecallResults").GetComponent<RecallResults>();

        displayResults();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void displayResults()
    {
        string resultsLeft = "";
        string resultsRight = "";

        resultsLeft = resultsLeft + "Number of correct answers: \n";
        resultsRight = resultsRight + recallResults.Correct.Count + "\n";

        resultsLeft = resultsLeft + "Number of incorrect answers: \n";
        resultsRight = resultsRight + recallResults.Incorrect.Count + "\n";

        resultsLeft = resultsLeft + "\n";
        resultsRight = resultsRight + "\n";

        resultsLeft = resultsLeft + "Details: \n";
        resultsRight = resultsRight + "\n";

        int noOfQuestions = recallResults.Correct.Count + recallResults.Incorrect.Count;

        // there are probably better way to sort through the answers
        // but given there is at most 6 words in a list complexity shouldnt be an issue
        for (int i = 0; i < noOfQuestions; i++)
        {
            string target = gameInfo.Words[i].text;
            bool found = false;

            resultsLeft = resultsLeft + target +"\n";

            foreach (string t in recallResults.Correct)
            {
                if (t == target)
                {
                    found = true;
                    resultsRight = resultsRight + "Correct \n";
                    break;
                }
            }
           

            if (found)
                continue;

            foreach (string t in recallResults.Incorrect)
            {
                if (t == target)
                {
                    resultsRight = resultsRight + "Incorrect \n";
                    break;
                }
            }
        }


        ResultsTextLeft.text = resultsLeft;
        ResultsTextRight.text = resultsRight;
    }


    public void NextStudent()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
    }

    public void SelectLanguage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
    }
}
