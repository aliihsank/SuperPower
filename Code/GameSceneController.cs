using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    //Non-Dependent Variables
    APIConnector apiConnector = null;

    int screenWidth, screenHeight;

    string email, password;

    public Camera mainCamera;

    List<GameObject> provinceObjects = null;

    int activeMap = 0; //Country Map(default), 1=Provinces Map


    //From Unity Inspector
    public Text dateTimeTextObj;

    public Text myCountryName;
    public Text myMoneyAmount;

    public GameObject countryLabelsGroup;
    public GameObject provinceLabelsGroup;

    public RectTransform provinceLabelsGroupTransform;
    public RectTransform countryLabelsGroupTransform;

    public GameObject settingsPanel;
    public GameObject militaryPanel;
    public GameObject diplomacyPanel;
    public GameObject lawsPanel;
    public GameObject budgetPanel;

    private GameObject activeUserOptionPanel;
    
    
    #region Camera Movements and Zoom-in,out Variables
    //Drag Camera
    private Vector3 dragOrigin; //Where are we moving?
    private Vector3 clickOrigin = Vector3.zero; //Where are we starting?
    private Vector3 basePos = Vector3.zero; //Where should the camera be initially?
    private Vector3 provinceLabelsGroupTransformPos = Vector3.zero;
    private Vector3 countryLabelsGroupTransformPos = Vector3.zero;
    #endregion


    //Database Connected Updatable
    Dictionary<int, Country> countries = new Dictionary<int, Country>();
    List<Province> provinces = new List<Province>();
    
    //Update Check locks:
    bool myCountryRdy = false, otherCountriesRdy = false;
    int provincesRequestCounter = 0;

    //Update Status Tracker
    string updateStatus = "finished";

    //UI Checks
    bool menuIsOpen = false;


    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;

        StartCoroutine(UpdateTime());

        //Load Saved Settings (only sound volume for now)
        AudioListener.volume = PlayerPrefs.GetFloat("soundVolume", 1);

        #region Set Non-Dependent Variables
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        
        //Get User email and password to memory
        GetUserSessionData();
        
        //Set variables and Objects
        apiConnector = new APIConnector(this);
        
        provinceObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Province"));
        #endregion
        
        //Start Continious Update of Infos
        StartCoroutine(StartUpdateInfos());
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

        #region Camera and Labels Movement
        float minPosX = -3.3f;
        float minPosY = -2.6f;
        float maxPosX = 5.7f;
        float maxPosY = 1.66f;

        //Camera Movements(Move camera and labels(country, province))
        if (!menuIsOpen)
        {
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
            
            //Keep them inside of limits
            if(newPositionVector.x > maxPosX || newPositionVector.x < minPosX)
            {
                newPositionVector.x = mainCamera.transform.position.x;
                newProvinceLabelGroupPositionVector.x = provinceLabelsGroupTransform.position.x;
                newcountryLabelGroupPositionVector.x = countryLabelsGroupTransform.position.x;
            }
            if(newPositionVector.y > maxPosY || newPositionVector.y < minPosY)
            {
                newPositionVector.y = mainCamera.transform.position.y;
                newProvinceLabelGroupPositionVector.y = provinceLabelsGroupTransform.position.y;
                newcountryLabelGroupPositionVector.y = countryLabelsGroupTransform.position.y;
            }

            //Assign new positions
            mainCamera.transform.position = newPositionVector;
            provinceLabelsGroupTransform.position = newProvinceLabelGroupPositionVector;
            countryLabelsGroupTransform.position = newcountryLabelGroupPositionVector;
        }
        #endregion
        
    }

    public IEnumerator UpdateTime()
    {
        while (true)
        {
            DateTime dateTime = DateTime.Now;
            dateTimeTextObj.GetComponent<Text>().text = dateTime.Date.ToLongDateString() + " - " + dateTime.ToShortTimeString();
            yield return new WaitForSeconds(10);
        }
    }

    public void ChangeSoundVolume(float soundVolume)
    {
        AudioListener.volume = soundVolume;
        PlayerPrefs.SetFloat("soundVolume", soundVolume);
    }

    public void LogOut()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadSceneAsync("LoginScene");
    }

    public void UserOptionClicked(string btnType)
    {
        menuIsOpen = true;

        switch (btnType)
        {
            case "Settings":
                activeUserOptionPanel = settingsPanel;
                break;
            case "Military":
                activeUserOptionPanel = militaryPanel;
                break;
            case "Diplomacy":
                activeUserOptionPanel = diplomacyPanel;
                break;
            case "Law":
                activeUserOptionPanel = lawsPanel;
                break;
            case "Budget":
                activeUserOptionPanel = budgetPanel;
                break;
        }

        if(activeMap == 0)
        {
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
        }
        else
        {
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
        }
        activeUserOptionPanel.SetActive(true);
    }

    public void CloseUserOptionPanel()
    {
        activeUserOptionPanel.SetActive(false);
        if(activeMap == 0)
        {
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
        }
        menuIsOpen = false;
    }

    public IEnumerator StartUpdateInfos()
    {
        string jsonToSend = @"{'email':'" + email + "', 'password':'" + password + "'}";
        
        while (true)
        {
            if (updateStatus == "finished")
            {
                updateStatus = "started";
                //Debug.Log("Update Started!");

                //Reset existing lists
                countries = new Dictionary<int, Country>();
                provinces = new List<Province>();

                //Get New Values and Set them to lists
                apiConnector.makeRequest(AddCountries, "myCountryDetails", jsonToSend);
                apiConnector.makeRequest(AddCountries, "otherCountriesDetails", jsonToSend);
                apiConnector.makeRequest(AddProvinces, "myProvincesDetails", jsonToSend);
                apiConnector.makeRequest(AddProvinces, "otherProvincesDetails", jsonToSend);
            }
            yield return new WaitForSeconds(60);
        }
    }

    public void ChangeMap()
    {
        if(activeMap == 0)
        {
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("btnChangeMap").GetComponentInChildren<Text>().text = "World Map";
            activeMap = 1;
        }
        else
        {
            provinceLabelsGroup.GetComponent<CanvasGroup>().alpha = 0;
            countryLabelsGroup.GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("btnChangeMap").GetComponentInChildren<Text>().text = "Province Map";
            activeMap = 0;
        }
    }


    public void LoadInfos2UI()
    {
        //Remove existing UI elements from scene-canvas'
        for(int i=0; i< countryLabelsGroup.transform.childCount; i++)
        {
            Destroy(countryLabelsGroup.transform.GetChild(i).gameObject);
        }
        countryLabelsGroup.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        countryLabelsGroup.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        for (int i = 0; i < provinceLabelsGroup.transform.childCount; i++)
        {
            Destroy(provinceLabelsGroup.transform.GetChild(i).gameObject);
        }
        provinceLabelsGroup.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        provinceLabelsGroup.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

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
            //Assign my country info to UI
            if (countries[countryX.Key].isMyCountry)
            {
                myCountryName.GetComponent<Text>().text = countries[countryX.Key].name;
                myMoneyAmount.GetComponent<Text>().text = countries[countryX.Key].remaining.ToString();
            }

            //Find centroid of provinces and assign country name to that position
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
        updateStatus = "finished";
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
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //Only 1 country
            if (details.IsObject)
            {
                //Debug.Log("AddMyCountry: " + details.ToString());
                countries.Add(details["id"], new Country(details["id"], details["cname"], details["totalpopulation"], details["avgTax"], details["numOfProvinces"], details["remaining"], true));
                myCountryRdy = true;
            }
            //List of Countries
            else
            {
                foreach (JSONNode country in details)
                {
                    //Debug.Log("AddOtherCountries: " + country.ToString());
                    countries.Add(country["id"], new Country(country["id"], country["cname"], country["totalpopulation"], country["avgTax"], country["numOfProvinces"], country["remaining"], false));
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
                //Debug.Log("AddProvinces: " + details.ToString());
                provinces.Add(new Province(details["id"], details["pname"], details["governorName"], details["population"], details["taxRate"], details["countryID"]));
            }
            //List of Provinces
            else
            {
                foreach (JSONNode province in details)
                {
                    //Debug.Log("AddProvinces: " + province.ToString());
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