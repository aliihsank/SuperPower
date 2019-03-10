using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using System;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{    
    public Camera mainCamera;

    private List<GameObject> provinceObjects = null;

    public Text myCountryName;
    public Text myMoneyAmount;

    public GameObject loadingImagePanel;
    public GameObject userPropertiesPanel;
    public GameObject dateTimePanel;
    public GameObject mapChoosePanel;
    public GameObject userOptionsPanel;

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
    #endregion
    

    void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        
        SetLoadingPage(0);
        
        AudioListener.volume = PlayerPrefs.GetFloat("soundVolume", 1);

        #region Set Non-Dependent Variables
        InterchangableVars.screenWidth = Screen.width;
        InterchangableVars.screenHeight = Screen.height;
        
        GetUserSessionData();
        
        provinceObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Province"));
        #endregion
        
    }
    
    void Update()
    {
        if (InterchangableVars.myCountryRdy && InterchangableVars.otherCountriesRdy && (InterchangableVars.provincesRequestCounter == 2))
        {
            LoadInfos2UI();

            InterchangableVars.myCountryRdy = false;
            InterchangableVars.otherCountriesRdy = false;
            InterchangableVars.provincesRequestCounter = 0;
        }

        #region Camera and Labels Movement
        float minPosX = -3.3f;
        float minPosY = -2.6f;
        float maxPosX = 5.7f;
        float maxPosY = 1.66f;

        //Camera Movements(Move camera and labels(country, province))
        if (!InterchangableVars.menuIsOpen)
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

    //This method works when every province and country info download is COMPLETED !!
    private void LoadInfos2UI()
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
            foreach (Province provinceInfo in InterchangableVars.provinces)
            {
                if (Int32.Parse(provinceObj.name) == provinceInfo.id)
                {
                    //Give Color to Country (every provinces of country same color)
                    Material[] prevMatArr = provinceObj.GetComponent<MeshRenderer>().materials;
                    Material newMaterial = new Material(prevMatArr[0]);
                    newMaterial.color = InterchangableVars.countries[provinceInfo.countryID].countryColor;
                    for (int i = 0; i < prevMatArr.Length; i++)
                    {
                        prevMatArr[i] = newMaterial;
                    }
                    provinceObj.GetComponent<MeshRenderer>().materials = prevMatArr;

                    //Load Province Labels
                    Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(provinceObj.transform.GetComponent<Renderer>().bounds.center);
                    if(InterchangableVars.screenWidth > InterchangableVars.screenHeight)
                    {
                        CreateLabel(InterchangableVars.screenWidth * ViewportPosition.x - InterchangableVars.screenWidth / 2, InterchangableVars.screenHeight * ViewportPosition.y - InterchangableVars.screenHeight / 2, provinceInfo.name.ToUpper() + "\n" + "P: " + provinceInfo.population + "\t" + "T: %" + provinceInfo.taxRate, 14, new Color(newMaterial.color.r + .5f, newMaterial.color.g + .5f, newMaterial.color.b + .5f, 1), provinceLabelsGroupTransform);
                    }
                    else
                    {
                        CreateLabel(InterchangableVars.screenHeight * ViewportPosition.x - InterchangableVars.screenHeight / 2, InterchangableVars.screenWidth * ViewportPosition.y - InterchangableVars.screenWidth / 2, provinceInfo.name.ToUpper() + "\n" + "P: " + provinceInfo.population + "\t" + "T: %" + provinceInfo.taxRate, 14, new Color(newMaterial.color.r + .5f, newMaterial.color.g + .5f, newMaterial.color.b + .5f, 1), provinceLabelsGroupTransform);
                    }
                }
            }
        }
        
        //Load Country Labels
        foreach (var countryX in InterchangableVars.provinces.GroupBy(x => x.countryID))
        {
            //Assign my country info to UI
            if (InterchangableVars.countries[countryX.Key].isMyCountry)
            {
                myCountryName.GetComponent<Text>().text = InterchangableVars.countries[countryX.Key].name;
                myMoneyAmount.GetComponent<Text>().text = InterchangableVars.countries[countryX.Key].remaining.ToString();
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

            if (InterchangableVars.screenWidth > InterchangableVars.screenHeight)
            {
                CreateLabel(InterchangableVars.screenWidth * countryCentroid.x - InterchangableVars.screenWidth / 2, InterchangableVars.screenHeight * countryCentroid.y - InterchangableVars.screenHeight / 2, InterchangableVars.countries[countryX.Key].name.ToUpper() + "\n" + "P: " + InterchangableVars.countries[countryX.Key].totalPopulation + "\t" + "N: " + InterchangableVars.countries[countryX.Key].numOfProvinces, 20, Color.black, countryLabelsGroupTransform);
            }
            else
            {
                CreateLabel(InterchangableVars.screenHeight * countryCentroid.x - InterchangableVars.screenHeight / 2, InterchangableVars.screenWidth * countryCentroid.y - InterchangableVars.screenWidth / 2, InterchangableVars.countries[countryX.Key].name.ToUpper() + "\n" + "P: " + InterchangableVars.countries[countryX.Key].totalPopulation + "\t" + "N: " + InterchangableVars.countries[countryX.Key].numOfProvinces, 20, Color.black, countryLabelsGroupTransform);
            }
        }


        SetLoadingPage(1);

        InterchangableVars.updateStatus = "finished";
    }


    /*
     * LOW LEVEL METHODS BELOW !!
     * 
     */

    private void SetLoadingPage(int status)
    {
        //status = 0 (not ready), 1 (ready)
        if (status == 0)
        {
            userPropertiesPanel.SetActive(false);
            dateTimePanel.SetActive(false);
            mapChoosePanel.SetActive(false);
            userOptionsPanel.SetActive(false);

            loadingImagePanel.SetActive(true);
        }
        else if (status == 1)
        {
            loadingImagePanel.SetActive(false);

            userPropertiesPanel.SetActive(true);
            dateTimePanel.SetActive(true);
            mapChoosePanel.SetActive(true);
            userOptionsPanel.SetActive(true);
        }
    }

    private GameObject CreateLabel(float x, float y, string text_to_print, int font_size, Color text_color, Transform parent)
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

    private void GetUserSessionData()
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
                    InterchangableVars.email = userInfo["email"];
                    InterchangableVars.password = userInfo["password"];
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