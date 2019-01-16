using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameDataScript : MonoBehaviour {

    // this class contains generic game setup details and functions
    // regardless of the actual content of game


    public int N_rows { get; private set; }
    public int N_cols { get; private set; }
    public int Iteration { get; private set; }
    public static GameDataScript instance;

    //These variables contain the number of rows and columns needed to arrange the cards at each level.
    private int[] Nrowslist = {2,2,2,2,2,3,4,4,4,4};
	private int[] Ncolslist = {1,2,3,4,5,4,4,5,5,5};
    public int TotalLevel { get; private set; }


    public int[] Ordering { get; private set; }
    //private static System.Random rng = new System.Random();

    // Use this for initialization
    void Start () {

        //When creating the game data, set the iteration to one (one card)
        Iteration = 1;
        TotalLevel = 1;

        arrange_grid(Iteration);
        //Scores = new int[TotalLevel]; //vector containing the score obtained for each level (initialized to 0)

        //Shuffle the cards
        //LoadExternalResources ext_res = GameObject.Find("LoadExternalResources").GetComponent<LoadExternalResources>();

        //Ordering = Enumerable.Range(0, ext_res.MemoryImages.Length).ToArray();
        //Shuffle(Ordering);
    }

    public void setOrdering(int len)
    {
        Ordering = Enumerable.Range(0, len).ToArray();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
	/*
	//Sets the score for a given iteration
	public void set_score(int iter,int score){
        Scores[iter-1] = score;
	}*/
	
    
	//Prepare for new level : increase iteration (the number of cards) by one and prepare the grid
    //not used in this situation, kept becuase other unused code reference this
	public void level_passed(){

        Iteration++;
        if (Iteration <= TotalLevel)
        {
            arrange_grid(Iteration);
            //if (Iteration > 8)
            //    Shuffle(Ordering);
        }
    }
	
	//Prepare the grid for arranging the cards, using the pre-set rows and columns number for each level.
	public void arrange_grid(int iteration){
        N_rows = Nrowslist[iteration-1];
        N_cols = Ncolslist[iteration-1];
	}
	
	public void Reset(){
	    Iteration = 1;
	    arrange_grid(Iteration);
        //Scores = new int[TotalLevel];
	}
	
	//This function ensures that the object is not destructed in between scenes
	void Awake() {
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
    }
	
    /*
	//Shuffling array function
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
	}*/
}
