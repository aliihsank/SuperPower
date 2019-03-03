using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Register : MonoBehaviour {

    public Text unameObj;
    public Text cnameObj;
    public Text emailObj;
    public Text passwordObj;

    private APIConnector apiConnector;
    
	void Start () {
        //Initialize APIConnector obj
        apiConnector = new APIConnector(this);
    }
	
	void Update () {
		
	}

    public void btnRegisterClicked()
    {
        string uname = unameObj.GetComponent<Text>().text;
        string cname = cnameObj.GetComponent<Text>().text;
        string email = emailObj.GetComponent<Text>().text;
        string password = passwordObj.GetComponent<Text>().text;

        if(uname != "" && cname != "" && email != "" && password != "")
        {
            string jsonToSend = @"{'uname':'" + uname + "', 'cname':'" + cname + "', 'email':'" + email + "', 'password':'" + password + "'}";

            apiConnector.makeRequest(CheckRegister, "userRegister", jsonToSend);

        }

    }

    public void CheckRegister(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            if (result["details"] == 1)
            {
                Debug.Log("Register Successful!");
                SceneManager.LoadSceneAsync("LoginScene");
            }
            else
            {
                Debug.Log("Register is NOT Successful !!");
                Debug.Log("Details: " + result["details"]);
            }
        }
        else
        {
            Debug.Log("Info:" + result["info"]);
        }
    }
}
