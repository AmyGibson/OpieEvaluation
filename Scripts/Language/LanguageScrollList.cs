using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class LanguageScrollList : MonoBehaviour {

    public Button prefab_button;
    private string[] itemList;
    public Transform contentPanel;


    // Use this for initialization
    void Start()
    {
        //The names of the profiles are defined by the directories created in the logging folder
        itemList = Directory.GetDirectories(Application.persistentDataPath + "/ExternalAssets");
        var foos = new List<string>(itemList);

        //Remove the instruction folder
        int i_to_remove = -1;
        for (int i = 0; i < itemList.Length; i++)
        {
            foos[i] = foos[i].Replace(Application.persistentDataPath + "/ExternalAssets/", "");
            if (foos[i].Equals("Instructions"))
                i_to_remove = i;
            
        }

        if (i_to_remove != -1)
            foos.RemoveAt(i_to_remove);

        itemList = foos.ToArray();

        //Create the buttons for each profile
        AddButtons();
        prefab_button.gameObject.SetActive(false);
    }

    //This function adds button to the List
    private void AddButtons()
    {
        for (int i = 0; i < itemList.Length; i++)
        {
            string language = itemList[i];

            Button newButton = Instantiate(prefab_button);
            newButton.transform.SetParent(contentPanel);

            ButtonLanguage sampleButton = newButton.GetComponent<ButtonLanguage>();
            sampleButton.Setup(language);
        }
    }

}
