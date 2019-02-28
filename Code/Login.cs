using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System;

public class Login : MonoBehaviour {

    public InputField email;
    public InputField password;

    public Button btnLogin;

    public string jsonToSend;

    public bool sessionChecked = false;
    public bool hasInternet = false;


    void Start () {
        //Make Connectivity Check
        APIConnector apiConnector = new APIConnector(this);
        string jsonToSend = @"{'email':'" + "null" + "', 'password':'" + "null" + "'}";
        apiConnector.makeRequest(CheckConnectivity, "/test", jsonToSend);

    }
	
	void Update () {

        //If device has internet connection then check for existing session
        if (hasInternet && !sessionChecked)
        {
            CheckSession();
            sessionChecked = true;
        }

    }

    public void CheckConnectivity(string request, JSONNode result)
    {
        if (result["Test Message(POST)"] == "Welcome to new API !!")
        {
            hasInternet = true;
        }
        else
        {
            Application.Quit();

        }
    }

    /*
     * Trigger : Login Button Pressed
     * Action : Check User by using API:
     *                              if exists => Save Session for later as JSON and change the scene to GameScene
     *                              if not    => Show a message in logger
     */
    public void BtnLoginPressed()
    {
        if (hasInternet)
        {
            jsonToSend = @"{'email':'" + email.text + "', 'password':'" + password.text + "'}";

            APIConnector superpowerConnector = new APIConnector(this);

            superpowerConnector.makeRequest(CheckLogin, "userLogin", jsonToSend);
        }
    }

    /*
     * Trigger : makeRequest
     * Action : Check result that comes from API and Process result
     */
    public void CheckLogin(string request, JSONNode result)
    {
        if(result["info"] == 1)
        {
            if(result["details"] == 1)
            {
                SaveUserInfo(request);
                SceneManager.LoadSceneAsync("GameScene");
            }
            else
            {
                Debug.Log("Details: " + result["details"]);
            }
        }
        else
        {
            Debug.Log("Info:" + result["info"]);
        }
    }

    /*
     * Trigger : CheckLogin
     * Action : If user login is successful, save it as json for later logins
     */
    public void SaveUserInfo(string dataAsJson)
    {
        PlayerPrefs.SetString("userInfo", dataAsJson);
    }
    
    /*
     * Trigger : Start
     * Action : Check user session: if exists any, login automatically
     */
    public void CheckSession()
    {
        string dataAsJson = PlayerPrefs.GetString("userInfo", "null");
        if (dataAsJson == "null")
        {
            Debug.Log("No user session found!");
        }
        else
        {
            JSONNode userInfo = JSON.Parse(dataAsJson);

            try
            {
                if (userInfo != null && !userInfo["email"].IsNull && !userInfo["password"].IsNull)
                {
                    SceneManager.LoadSceneAsync("GameScene");
                }
            }
            catch (Exception ee)
            {
                Debug.LogError("Error: User info was saved in wrong format!");
                Debug.LogError(ee.StackTrace);
            }
        }
    }
}

