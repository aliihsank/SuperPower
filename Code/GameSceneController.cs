using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System;

public class GameSceneController : MonoBehaviour
{
    public string email, password;

    public Camera mainCamera;

    #region Camera Movements and Zoom-in,out Variables
    //Drag Camera
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 clickOrigin = Vector3.zero; //Where are we starting?
    private Vector3 basePos = Vector3.zero; //Where should the camera be initially?

    //Zoom-in out
    public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
    #endregion

    APIConnector apiConnector = null;

    Dictionary<int, Country> countries = new Dictionary<int, Country>();
    List<Province> provinces = new List<Province>();
    List<GameObject> provinceObjects = null;

    //Check locks:
    bool myCountryRdy = false, otherCountriesRdy = false;
    int provincesRequestCounter = 0;


    void Start()
    {
        //Get User email and password to memory
        GetUserSessionData();

        string jsonToSend = @"{'email':'" + email + "', 'password':'" + password + "'}";

        //Set variables and Objects
        apiConnector = new APIConnector(this);

        //Initiate variables
        
        apiConnector.makeRequest(AddCountries, "myCountryDetails", jsonToSend);
        apiConnector.makeRequest(AddCountries, "otherCountriesDetails", jsonToSend);
        apiConnector.makeRequest(AddProvinces, "myProvicesDetails", jsonToSend);
        apiConnector.makeRequest(AddProvinces, "otherProvincesDetails", jsonToSend);
        
        provinceObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Province"));
        

    }

    void Update()
    {
        #region Camera Movemet and Zoom-in,out
        /*
         * Camera position range, for now :
         * cameraX = -5.5 , 4.5
         * cameraY = -0.5 , 2.5
         */

        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // If the camera is orthographic...
            if (mainCamera.orthographic)
            {
                // ... change the orthographic size based on the change in distance between the touches.
                mainCamera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Make sure the orthographic size never drops below zero.
                mainCamera.orthographicSize = Mathf.Max(mainCamera.orthographicSize, 0.1f);
            }
            else
            {
                // Otherwise change the field of view based on the change in distance between the touches.
                mainCamera.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

                // Clamp the field of view to make sure it's between 0 and 180.
                mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, 0.1f, 179.9f);
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (clickOrigin == Vector3.zero)
                {
                    clickOrigin = Input.mousePosition;
                    basePos = mainCamera.transform.position;
                }
                dragOrigin = Input.mousePosition;
            }

            if (!Input.GetMouseButton(0))
            {
                clickOrigin = Vector3.zero;
                return;
            }
            Vector3 newPositionVector = new Vector3(basePos.x + ((clickOrigin.x - dragOrigin.x) * .01f), basePos.y + ((clickOrigin.y - dragOrigin.y) * .01f), -10);

            if (newPositionVector.x > 4.5 || newPositionVector.x < -5.5 || newPositionVector.y > 2.5 || newPositionVector.y < -0.5)
            {
                Debug.Log("Out Of Bounds!");
            }
            else
            {
                mainCamera.transform.position = newPositionVector;
            }
        }
        #endregion


        if(myCountryRdy && otherCountriesRdy && provincesRequestCounter == 2)
        {
            LoadInfos2UI();

            myCountryRdy = false;
            otherCountriesRdy = false;
            provincesRequestCounter = 0;
        }
    }


    public void LoadInfos2UI()
    {
        //Load data to UI objects
        foreach (GameObject provinceObj in provinceObjects)
        {
            foreach (Province provinceInfo in provinces)
            {
                if (Int32.Parse(provinceObj.name) == provinceInfo.id)
                {
                    Material[] prevMatArr = provinceObj.GetComponent<MeshRenderer>().materials;
                    Material newMaterial = new Material(prevMatArr[0]);
                    newMaterial.color = countries[provinceInfo.countryID].countryColor;
                    for (int i = 0; i < prevMatArr.Length; i++)
                    {
                        prevMatArr[i] = newMaterial;
                    }
                    provinceObj.GetComponent<MeshRenderer>().materials = prevMatArr;
                }
            }
        }
    }

    public void AddCountries(string request, JSONNode result)
    {
        if(result["info"] == 1)
        {
            JSONNode details = result["details"];

            //Only 1 country
            if (details.IsObject)
            {
                Debug.Log("AddCountries: " + details.ToString());
                countries.Add(details["id"], new Country(details["id"], details["name"], details["totalPopulation"], details["avgTax"], details["numOfProvinces"], details["remaining"]));
                myCountryRdy = true;
            }
            //List of Countries
            else
            {
                foreach (JSONNode country in details)
                {
                    Debug.Log("AddCountries: " + country.ToString());
                    countries.Add(country["id"], new Country(country["id"], country["name"], country["totalPopulation"], country["avgTax"], country["numOfProvinces"], country["remaining"]));
                }
                otherCountriesRdy = true;
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
        }
    }

    public void AddProvinces(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //Only 1 province
            if (details.IsObject)
            {
                Debug.Log("AddProvinces: " + details.ToString());
                provinces.Add(new Province(details["id"], details["name"], details["governorName"], details["population"], details["taxRate"], details["countryID"]));
            }
            //List of Provinces
            else
            {
                foreach (JSONNode province in details)
                {
                    Debug.Log("AddProvinces: " + province.ToString());

                    provinces.Add(new Province(province["id"], province["name"], province["governorName"], province["population"], province["taxRate"], province["countryID"]));
                }
            }
            provincesRequestCounter++;
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
        }
    }
    

    public void GetUserSessionData()
    {
        string dataAsJson = PlayerPrefs.GetString("userInfo", "null");
        if (dataAsJson == "null")
        {
            Debug.Log("No user session found!");
        }
        else
        {
            JSONNode userInfo = JSON.Parse(dataAsJson.Replace("'", "\""));
            try
            {
                if (userInfo != null && !userInfo["email"].IsNull && !userInfo["password"].IsNull)
                {
                    this.email = userInfo["email"];
                    this.password = userInfo["password"];
                }
                else
                {
                    Debug.LogError("Something wrong with json string!");
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