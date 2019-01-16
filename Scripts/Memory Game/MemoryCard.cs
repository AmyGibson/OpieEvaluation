using UnityEngine;
using System.Collections;

public class MemoryCard : MonoBehaviour {
	[SerializeField] private GameObject cardBack = null;    
	[SerializeField] private SceneController controller = null;
	[SerializeField] private AudioClip cardFlipSound = null;
	private string transcription = null;

	private AudioClip cardSound = null;

	private int _id;
	public int id {
		get {return _id;}
	}

	private GameObject get_card_front()
	{
		for (int i = 0; i < gameObject.transform.childCount; i++) {
			GameObject child = gameObject.transform.GetChild(i).gameObject;
			if (child != cardBack) {
				return child;
			}
		}

		return null;
	}

	//Sets the ID number, image, sound and transcription of the card
	public void SetCard(int id, Sprite image, AudioClip cardSound, string transc) {
		_id = id;
		
		SpriteRenderer sr = get_card_front().GetComponent<SpriteRenderer>();
		sr.sprite = image;
		this.cardSound = cardSound;
		this.transcription = transc;
	}

	//Plays the card
	private IEnumerator flipAndPlay()
	{
		//Display the transcription of the card and plays its associated sound
		controller.SetTranscription(this.transcription);
		GetComponent<AudioSource>().PlayOneShot (cardFlipSound);
		
		//Disables the card back to make the card image visible
		cardBack.SetActive(false);

		controller.CardRevealed (this);

		//Wait for sound to be finished
		while (GetComponent<AudioSource> ().isPlaying) {
			yield return null;
		}
		
		//Send a finish event to enable transition
		controller.Finish ();


		GetComponent<AudioSource> ().PlayOneShot (cardSound);

		while (GetComponent<AudioSource> ().isPlaying) {
			yield return null;	
		}
		controller.SetTranscription("");
		controller.Finish ();
		yield break;
	}

	public void StartFlipAndPlay()
	{
		StartCoroutine (flipAndPlay ());
	}

	public void OnMouseDown() {
		if (cardBack.activeInHierarchy) {
			controller.SetCurrentCard (this);
			controller.Click ();
		}
	}

	private IEnumerator flipBack()
	{
		
		GetComponent<AudioSource>().PlayOneShot (cardFlipSound);
		cardBack.SetActive(true);

		while (GetComponent<AudioSource> ().isPlaying) {
			yield return null;
		}

		controller.Finish ();
	}

	public void Unreveal() {
		Debug.Log ("flip back");
		StartCoroutine (flipBack ());
	}
}
