using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    int screenWidth, screenHeight;

    string email, password;

    public Camera mainCamera;

    public GameObject countryLabelsGroup;
    public GameObject provinceLabelsGroup;

    public RectTransform provinceLabelsGroupTransform;
    public RectTransform countryLabelsGroupTransform;
        
    #region Camera Movements and Zoom-in,out Variables
    //Drag Camera
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 clickOrigin = Vector3.zero; //Where are we starting?
    private Vector3 basePos = Vector3.zero; //Where should the camera be initially?
    private Vector3 provinceLabelsGroupTransformPos = Vector3.zero;
    private Vector3 countryLabelsGroupTransformPos = Vector3.zero;

    //Zoom-in out
    public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
    #endregion

    APIConnector apiConnector = null;

    Dictionary<int, Country> countries = new Dictionary<int, Country>();
    List<Province> provinces = new List<Province>();
    List<GameObject> provinceObjects = null;

    int activeMap = 0; //Country Map(default), 1=Provinces Map

    //Check locks:
    bool myCountryRdy = false, otherCountriesRdy = false;
    int provincesRequestCounter = 0;

    //Update Tracker
    int updateNum = 0;


    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        
        //Get User email and password to memory
        GetUserSessionData();

        string jsonToSend = @"{'email':'" + email + "', 'password':'" + password + "'}";

        //Set variables and Objects
        apiConnector = new APIConnector(this);

        //Initiate variables (First Time)
        apiConnector.makeRequest(AddCountries, "myCountryDetails", jsonToSend);
        apiConnector.makeRequest(AddCountries, "otherCountriesDetails", jsonToSend);
        apiConnector.makeRequest(AddProvinces, "myProvincesDetails", jsonToSend);
        apiConnector.makeRequest(AddProvinces, "otherProvincesDetails", jsonToSend);
        
        provinceObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Province"));
        
    }

    void Update()
    {
        //UI Update Trigger (Also check update num)
        if (myCountryRdy && otherCountriesRdy && (provincesRequestCounter == 2))
        {
            LoadInfos2UI();

            myCountryRdy = false;
            otherCountriesRdy = false;
            provincesRequestCounter = 0;
        }


        #region Camera Movemet
        
        //Camera Movements(Move camera and labels(country, province))
        if (Input.GetMouseButton(0))
        {
            if (clickOrigin == Vector3.zero)
            {
                clickOrigin = Input.mousePosition;
                basePos = mainCamera.transform.position;
                provinceLabelsGroupTransformPos = provinceLabelsGroupTransform.position;
                countryLabelsGroupTransformPos = countryLabelsGroupTransform.position;
            }
            dragOrigin = Input.mousePosition;
        }

        if (!Input.GetMouseButton(0))
        {
            clickOrigin = Vector3.zero;
            return;
        }

        Vector3 newPositionVector = new Vector3(basePos.x + ((clickOrigin.x - dragOrigin.x) * (3.57f / Screen.width)), basePos.y + ((clickOrigin.y - dragOrigin.y) * (3.57f / Screen.width)), -10);
        Vector2 newProvinceLabelGroupPositionVector = new Vector2(provinceLabelsGroupTransformPos.x - (clickOrigin.x - dragOrigin.x), provinceLabelsGroupTransformPos.y - (clickOrigin.y - dragOrigin.y));
        Vector2 newcountryLabelGroupPositionVector = new Vector2(countryLabelsGroupTransformPos.x - (clickOrigin.x - dragOrigin.x), countryLabelsGroupTransformPos.y - (clickOrigin.y - dragOrigin.y));

        mainCamera.transform.position = newPositionVector;
        provinceLabelsGroupTransform.position = newProvinceLabelGroupPositionVector;
        countryLabelsGroupTransform.position = newcountryLabelGroupPositionVector;
        #endregion
        
    }

    public void ChangeMap()
    {
        if(activeMap == 0)
        {
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
            Button btnChangeMap = GameObject.FindGameObjectWithTag("btnChangeMap").GetComponent<Button>();
            btnChangeMap.GetComponent<Text>().text = "World Map";
            activeMap = 1;
        }
        else
        {
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
            Button btnChangeMap = GameObject.FindGameObjectWithTag("btnChangeMap").GetComponent<Button>();
            btnChangeMap.GetComponent<Text>().text = "Province Map";
            activeMap = 0;
        }
    }


    public void LoadInfos2UI()
    {
        //Load Province Labels and give color to CountryObjs'
        foreach (GameObject provinceObj in provinceObjects)
        {
            foreach (Province provinceInfo in provinces)
            {
                if (Int32.Parse(provinceObj.name) == provinceInfo.id)
                {
                    //Give Color to Country (every provinces of country same color)
                    Material[] prevMatArr = provinceObj.GetComponent<MeshRenderer>().materials;
                    Material newMaterial = new Material(prevMatArr[0]);
                    newMaterial.color = countries[provinceInfo.countryID].countryColor;
                    for (int i = 0; i < prevMatArr.Length; i++)
                    {
                        prevMatArr[i] = newMaterial;
                    }
                    provinceObj.GetComponent<MeshRenderer>().materials = prevMatArr;

                    //Load Province Labels
                    Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(provinceObj.transform.GetComponent<Renderer>().bounds.center);
                    if(screenWidth > screenHeight)
                    {
                        CreateLabel(screenWidth * ViewportPosition.x - screenWidth / 2, screenHeight * ViewportPosition.y - screenHeight / 2, provinceInfo.name.ToUpper() + "\n" + "P: " + provinceInfo.population + "\t" + "T: %" + provinceInfo.taxRate, 14, new Color(newMaterial.color.r + .5f, newMaterial.color.g + .5f, newMaterial.color.b + .5f, 1), provinceLabelsGroupTransform);
                    }
                    else
                    {
                        CreateLabel(screenHeight * ViewportPosition.x - screenHeight / 2, screenWidth * ViewportPosition.y - screenWidth / 2, provinceInfo.name.ToUpper() + "\n" + "P: " + provinceInfo.population + "\t" + "T: %" + provinceInfo.taxRate, 14, new Color(newMaterial.color.r + .5f, newMaterial.color.g + .5f, newMaterial.color.b + .5f, 1), provinceLabelsGroupTransform);
                    }
                }
            }
        }
        
        //Load Country Labels
        foreach (var countryX in provinces.GroupBy(x => x.countryID))
        {
            Vector2 countryCentroid = Vector2.zero;

            //Find centroid of provinces
            foreach(var provinceInCountryX in countryX)
            {
                Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(provinceObjects.Find(x => x.name == provinceInCountryX.id.ToString()).transform.GetComponent<Renderer>().bounds.center);
                countryCentroid += ViewportPosition;
            }
            countryCentroid = countryCentroid / countryX.Count();

            if (screenWidth > screenHeight)
            {
                CreateLabel(screenWidth * countryCentroid.x - screenWidth / 2, screenHeight * countryCentroid.y - screenHeight / 2, countries[countryX.Key].name.ToUpper() + "\n" + "P: " + countries[countryX.Key].totalPopulation + "\t" + "N: " + countries[countryX.Key].numOfProvinces, 20, Color.black, countryLabelsGroupTransform);
            }
            else
            {
                CreateLabel(screenHeight * countryCentroid.x - screenHeight / 2, screenWidth * countryCentroid.y - screenWidth / 2, countries[countryX.Key].name.ToUpper() + "\n" + "P: " + countries[countryX.Key].totalPopulation + "\t" + "N: " + countries[countryX.Key].numOfProvinces, 20, Color.black, countryLabelsGroupTransform);
            }
        }
        

    }



    /*
     * LOW LEVEL METHODS BELOW !!
     * 
     */

    GameObject CreateLabel(float x, float y, string text_to_print, int font_size, Color text_color, Transform parent)
    {
        GameObject UItextGO = new GameObject(text_to_print);
        
        UItextGO.transform.SetParent(parent);

        RectTransform trans = UItextGO.AddComponent<RectTransform>();
        trans.anchoredPosition = new Vector2(x, y);

        Text text = UItextGO.AddComponent<Text>();
        text.text = text_to_print;
        text.font = Font.CreateDynamicFontFromOSFont("Arial", font_size);
        text.fontSize = font_size;
        text.color = text_color;
        text.alignment = TextAnchor.MiddleCenter;

        ContentSizeFitter fitter = UItextGO.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        return UItextGO;
    }

    public void AddCountries(string request, JSONNode result)
    {
        if(result == null)
        {
            Debug.Log("result null: " + request);
        }
        else
        {
            Debug.Log("Info var mı: " + request + " : " + result);
        }

        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //Only 1 country
            if (details.IsObject)
            {
                Debug.Log("AddMyCountry: " + details.ToString());
                countries.Add(details["id"], new Country(details["id"], details["cname"], details["totalpopulation"], details["avgTax"], details["numOfProvinces"], details["remaining"]));
                myCountryRdy = true;
            }
            //List of Countries
            else
            {
                foreach (JSONNode country in details)
                {
                    Debug.Log("AddOtherCountries: " + country.ToString());
                    countries.Add(country["id"], new Country(country["id"], country["cname"], country["totalpopulation"], country["avgTax"], country["numOfProvinces"], country["remaining"]));
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
                provinces.Add(new Province(details["id"], details["pname"], details["governorName"], details["population"], details["taxRate"], details["countryID"]));
            }
            //List of Provinces
            else
            {
                foreach (JSONNode province in details)
                {
                    Debug.Log("AddProvinces: " + province.ToString());
                    provinces.Add(new Province(province["id"], province["pname"], province["governorName"], province["population"], province["taxRate"], province["countryID"]));
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