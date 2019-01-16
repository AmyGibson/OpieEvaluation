using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;

public class ConnectionDialog : MonoBehaviour
{

    public Text ipAddress;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void AddDigit (string digit)
	{
		ipAddress.text = ipAddress.text + digit;
	}

	public void DeleteDigit ()
	{
		ipAddress.text = ipAddress.text.Remove (ipAddress.text.Length - 1);
	}

	public void invalidIP ()
	{
		ipAddress.text = "INVALID";
	}

	public void Connect ()
	{
		//networkControl.ConnectToFace (ipAddress.text, this);

	}

	public void Destroy ()
	{
		Destroy (this.gameObject);
	}
}
