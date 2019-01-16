using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class WifiAndroidManager : MonoBehaviour {

    // original: "\"NgukurrOpie\"","\"21157233\""
    //private const string wifi_ssid = "\"NgukurrOpie2\"";
    //private const string wifi_pwd = "\"22232113\"";

    private const string wifi_ssid = "\"NgukurrOpie\"";
    private const string wifi_pwd = "\"21157233\"";

    // Use this for initialization

    void Start () {


#if UNITY_ANDROID
        Debug.Log("Started Wifi manager");
        connectWifi(wifi_ssid, wifi_pwd);
#endif

    }

    // Update is called once per frame
    void Update () {

#if UNITY_EDITOR_WIN
        UnityEngine.SceneManagement.SceneManager.LoadScene("language_selection");
#endif


#if UNITY_ANDROID
        if (!isWifiEnabled())
            Debug.Log("wifi not enable");

        // make sure there is a valid ip before proceeding
        if (wifiInfo().Call<int>("getIpAddress") != 0)
        {
            /*if ((wifiInfo().Call<String>("getSSID") != wifi_ssid))
            {
                Debug.Log("ssid doesnt match " + wifiInfo().Call<String>("getSSID").ToString());
            }*/

                //Check if wifi is connected to "Ngukurr Opie", and switch to Root scene if it is
            if ((wifiInfo().Call<String>("getSSID") == wifi_ssid) && isWifiEnabled())
            {   
                 UnityEngine.SceneManagement.SceneManager.LoadScene("RootScene");
            }
        }
#endif

    }


    public void reattempt_connexion(){
		//Name and password of the connection
		connectWifi(wifi_ssid, wifi_pwd);
	}
	
	public bool setWifiEnabled(bool enabled)
	{
    using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
    {
        try
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                return wifiManager.Call<bool>("setWifiEnabled", enabled);
            }
        }
        catch (Exception e)
        {
        }
    }
    return false;
}

public bool isWifiEnabled()
{
    using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
    {
        try
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                return wifiManager.Call<bool>("isWifiEnabled");
            }
        }
        catch (Exception e)
        {

        }
    }
    return false;
}

public AndroidJavaObject wifiInfo()
	{
    using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
    {
        try
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                return wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
            }
        }
        catch (Exception e)
        {
        }
    }
    return null;
}


//This function looks for the wifi network called NAME, and tries to connect to it using the password.
public void connectWifi(string name, string password)
	{
    using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
    {
        try
        {
			//Define the wifi config object containing network information (incl. password)
			AndroidJavaObject wifiConfig = new AndroidJavaObject("android.net.wifi.WifiConfiguration");
			wifiConfig.Set<String>("SSID", name);
			wifiConfig.Set<String>("preSharedKey", password);
			
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
				int netId = wifiManager.Call<int>("addNetwork",wifiConfig);
				Debug.Log(netId.ToString());
				//Disconnect from current network
				wifiManager.Call<bool>("disconnect");
				//Set new network to the wificonfig object
				wifiManager.Call<bool>("enableNetwork",netId,true);
				//Activate wifi connection again
				wifiManager.Call<bool>("reconnect");
				Debug.Log("connection done");
                //return wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
            }
        }
        catch (Exception e)
        {
        }
    }
}
    public void quit_app()
    {
        Application.Quit();
    }
}
