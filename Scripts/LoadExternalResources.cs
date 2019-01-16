using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class LoadExternalResources : MonoBehaviour {


    public Text msg;
    private string language;

    private Sprite[] _memoryImages = null;
    public Sprite[] MemoryImages {
        get { return _memoryImages; }
    }
    private AudioClip[] _memorySounds = null;
    public AudioClip[] MemorySounds
    {
        get { return _memorySounds; }
    }
    private string[] _memoryWords = null;
    public string[] MemoryWords
    {
        get { return _memoryWords; }
    }


    private Sprite[] _storyImages = null;
    public Sprite[] StoryImages
    {
        get { return _storyImages; }
    }
    private AudioClip[] _storySounds = null;
    public AudioClip[] StorySounds
    {
        get { return _storySounds; }
    }
    private Sprite[] _storyMasks = null;
    public Sprite[] StoryMasks
    {
        get { return _storyMasks; }
    }

    //these are just for timing for debugging
    private float soundST;

    // making thing a singleton instead of DontDestroyOnLoad,
    // otherwise duplicates will be created everytime this scene is load
    public static LoadExternalResources instance;

    private void Awake()
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
        
    }


    // check if a subfolder exist, may need to futher check if it is empty
    public bool CheckIfPicturesExist() {
        language = GetCurrentLanguage();
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/pictures";

        if (Directory.Exists(path))
            return true;
        return false;
    }

    public bool CheckIfStoryExist()
    {
        language = GetCurrentLanguage();
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/pictures";
        if (Directory.Exists(path))
            return true;
        return false;
    }



    // a more efficient way to import resource from external is by using corountine
    // but it was a bit difficult to make sure all corountine are done before progressing
    // to investigate again if time permits
    /*
    private void LoadMemoryImages()
    {
        //float startTime = Time.realtimeSinceStartup;
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/pictures";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _memoryImages = new Sprite[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;
                Texture2D tex = new Texture2D(400, 300); //empty texture
                using (WWW www = new WWW(url))
                {
                    www.LoadImageIntoTexture(tex);
                    _memoryImages[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading memory game pictures \n");
            Debug.Log(e);
        }
        //Debug.Log("LoadMemoryImages time" + (Time.realtimeSinceStartup - startTime).ToString());
    }*/

    private void LoadMemoryImages()
    {
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/pictures";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _memoryImages = new Sprite[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;
                StartCoroutine(LoadMemoryOneImage(url, i));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading memory game pictures \n");
            Debug.Log(e);
        }
    }

    private IEnumerator LoadMemoryOneImage(string url, int i)
    {
        Texture2D tex = new Texture2D(4, 4);
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            _memoryImages[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        }

    }

    private bool CheckAllMemoryImages()
    {
        if (_memoryImages == null)
            return false;
        for (int i = 0; i < _memoryImages.Length; i++)
        {
            if (_memoryImages[i] == null)
                return false;
        }
        return true;
    }

    private void LoadMemorySounds()
    {
        //soundST = Time.realtimeSinceStartup;
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/sounds";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _memorySounds = new AudioClip[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;
                StartCoroutine(LoadOneSound(url, i));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading memory game sounds \n");
            Debug.Log(e);
        }
    }

    private IEnumerator LoadOneSound(string url, int i)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            _memorySounds[i] = www.GetAudioClip();
        }

    }


    private void LoadMemoryWords()
    {
        //float startTime = Time.realtimeSinceStartup;
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/words";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _memoryWords = new string[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;

                using (WWW www = new WWW(url))
                {
                    _memoryWords[i] = www.text;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading memory game words \n");
            Debug.Log(e);
        }
       // Debug.Log("LoadMemoryWords time" + (Time.realtimeSinceStartup - startTime).ToString());

    }

    public bool IsLoaded() {
        if (_memoryImages == null)
            return false;
        return true;
    }


    public void Reset()
    {
        _memoryImages = null;
        _memorySounds = null;
        _memoryWords = null;
        _storyImages = null;
        _storySounds = null;
        _storyMasks = null;
    }

    private string GetCurrentLanguage() {
        return GameObject.Find("ProfileInfo").GetComponent<LogInfo>().GetLanguageName();
    }
    
    public void LoadMemoryGameResources() {
        
        language = GetCurrentLanguage();
        LoadMemoryImages();
        LoadMemorySounds();
        LoadMemoryWords();
    }



    public bool AreResourcesReady()
    {
        if (!CheckAllMemoryImages())
            return false;
        else
            return CheckIfAllSoundsLoaded();
    }

    private bool CheckIfAllSoundsLoaded()
    {
        if (_memorySounds == null)
            return false;
        bool alldone = true;
        for (int i = 0; i < _memorySounds.Length; i++)
        {
            if (_memorySounds[i] == null)
            {
                alldone = false;
                break;
            }
            AudioDataLoadState res = _memorySounds[i].loadState;
            if (res != AudioDataLoadState.Loaded)
            {
                alldone = false;
                msg.text = "waiting to load sound " + i.ToString();
                break;
            }
        }
        //if (alldone)
         //   Debug.Log("LoadMemorySound time" + (Time.realtimeSinceStartup - soundST).ToString());
        return alldone;

    }


    /*
    private void LoadStoryImages()
    {
        float startTime = Time.realtimeSinceStartup;
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/pictures";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _storyImages = new Sprite[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;

                Texture2D tex = new Texture2D(4, 4);
                //Texture2D tex = new Texture2D(960, 720, TextureFormat.RGBA32, false); //empty texture
                using (WWW www = new WWW(url))
                {
                    www.LoadImageIntoTexture(tex);
                    // setting the sprit to fullrect reduce texture loading time
                    _storyImages[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), 
                        new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading story pictures \n");
            Debug.Log(e);
        }
        Debug.Log("LoadStoryImages time" + (Time.realtimeSinceStartup - startTime).ToString());
    }*/


    private void LoadStoryImages()
    {
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/pictures";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        
        FileInfo[] fileinfo = dataDir.GetFiles();
        _storyImages = new Sprite[fileinfo.Length];

        for (int i = 0; i < fileinfo.Length; i++)
        {
            string url = "file:///" + path + "/" + fileinfo[i].Name;
            StartCoroutine(LoadStoryOneImage(url, i));
        }
        
    }


    private IEnumerator LoadStoryOneImage(string url, int i)
    {
        Texture2D tex = new Texture2D(4, 4);
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            _storyImages[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        }

    }


    private bool CheckAllStoryImages() {
        if (_storyImages == null)
            return false;
        for (int i = 0; i < _storyImages.Length; i++) {
            if (_storyImages[i] == null)
                return false;
        }
        return true;
    }

    /*
    private void LoadStoryMasks()
    {
        float startTime = Time.realtimeSinceStartup;
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/masks";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _storyMasks = new Sprite[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;
                Texture2D tex = new Texture2D(4, 4); //empty texture
                using (WWW www = new WWW(url))
                {
                    www.LoadImageIntoTexture(tex);
                    _storyMasks[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading story masks \n");
            Debug.Log(e);
        }

        Debug.Log("LoadStoryMasks time" + (Time.realtimeSinceStartup - startTime).ToString());
    }*/

    private void LoadStoryMasks()
    {
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/masks";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        FileInfo[] fileinfo = dataDir.GetFiles();
        _storyMasks = new Sprite[fileinfo.Length];

        for (int i = 0; i < fileinfo.Length; i++)
        {
            string url = "file:///" + path + "/" + fileinfo[i].Name;
            StartCoroutine(LoadStoryOneMask(url, i));
        }
    }


    private IEnumerator LoadStoryOneMask(string url, int i)
    {
        Texture2D tex = new Texture2D(4, 4);
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            _storyMasks[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
        }

    }


    private bool CheckAllStoryMasks()
    {
        if (_storyMasks == null)
            return false;
        for (int i = 0; i < _storyMasks.Length; i++)
        {
            if (_storyMasks[i] == null)
                return false;
        }
        return true;
    }


    private void LoadStorySounds()
    {
        string path = Application.persistentDataPath + "/ExternalAssets/" + language + "/story/sounds";

        DirectoryInfo dataDir = new DirectoryInfo(path);
        try
        {
            FileInfo[] fileinfo = dataDir.GetFiles();
            _storySounds = new AudioClip[fileinfo.Length];

            for (int i = 0; i < fileinfo.Length; i++)
            {
                string url = "file:///" + path + "/" + fileinfo[i].Name;
                StartCoroutine(LoadStoryOneSound(url, i));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Fail in loading story sounds \n");
            Debug.Log(e);
        }
    }

    private IEnumerator LoadStoryOneSound(string url, int i)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            _storySounds[i] = www.GetAudioClip();
        }

    }


    public void LoadStoryGameResources()
    {

        language = GetCurrentLanguage();
        LoadStoryImages();
        LoadStorySounds();
        LoadStoryMasks();
    }



    public bool AreStoryResourcesReady()
    {
        if (!CheckAllStoryImages())
            return false;
        else if (!CheckAllStoryMasks())
            return false;
        else
            return CheckIfAllStorySoundsLoaded();
    }

    private bool CheckIfAllStorySoundsLoaded()
    {
        if (_storySounds == null)
            return false;
        bool alldone = true;
        for (int i = 0; i < _storySounds.Length; i++)
        {
            if (_storySounds[i] == null)
            {
                alldone = false;
                break;
            }
            AudioDataLoadState res = _storySounds[i].loadState;
            if (res != AudioDataLoadState.Loaded)
            {
                alldone = false;
                msg.text = "waiting to load sound " + i.ToString();
                break;
            }
        }
        
        return alldone;

    }




    // Use this for initialization
    void Start () {
        Reset();
    }
	
	// Update is called once per frame
	void Update () {
      
    }

  




}
