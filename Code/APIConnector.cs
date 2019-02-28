using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Author : Ali İhsan Karabal
 * Requirement : SimleJSON
 * First Release : 23.02.2019
 */


public class APIConnector {

    private MonoBehaviour context;

    //Base Uri as default:
    private string baseUri = "http://superpowerapi-env.cyhwffmpct.eu-central-1.elasticbeanstalk.com/";
    
    public APIConnector()
    {

    }

    public APIConnector(MonoBehaviour context)
    {
        this.context = context;
    }


    //GENERIC MAKE REQUEST METHOD FOR GENERATING ANY REQUEST IN DIFFERENT THREAD
    public void makeRequest(System.Action<string, JSONNode> responseHandler, string requestedUri, string jsonBody)
    {
        if(context == null)
        {
            throw new UnassignedReferenceException("Please specify MonoBehaviour context class.");
        }
        context.StartCoroutine(PostRequest(callBack =>
        {
            if (callBack != null)
            {
                JSONNode result = JSON.Parse(callBack);
                responseHandler(jsonBody, result);
            }
        }, requestedUri, jsonBody));
    }

    //GENERIC MAKE REQUEST METHOD FOR GENERATING ANY REQUEST IN DIFFERENT THREAD
    public void makeRequest(MonoBehaviour context, System.Action<string, JSONNode> responseHandler, string requestedUri, string jsonBody)
    {
        this.context = context;
        context.StartCoroutine(PostRequest(callBack =>
        {
            if (callBack != null)
            {
                JSONNode result = JSON.Parse(callBack);
                responseHandler(jsonBody, result);
            }
        }, requestedUri, jsonBody));
    }


    //GENERIC POST METHOD FOR SENDING JSON DATA
    private IEnumerator PostRequest(System.Action<string> callBack, string requestedUri, string bodyJsonString)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("content-type", "application/json");
        headers.Add("content-length", bodyJsonString.Length.ToString());

        bodyJsonString = bodyJsonString.Replace("'", "\"");

        byte[] postData = System.Text.Encoding.UTF8.GetBytes(bodyJsonString);

        WWW www = new WWW(baseUri + requestedUri, postData, headers);

        yield return www;
        
        callBack(www.text);
    }

}
