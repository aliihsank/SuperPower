using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class UserOptionChange : MonoBehaviour
{
    public GameObject transparentPanel;

    public GameObject giveMissionPanel;
    public GameObject newAggrementPanel;
    public GameObject makeLawPanel;
    public GameObject setBudgetPanel;

    private GameObject activeUserOptionChangePanel;


    /*
     * Give Mission Parameters
     */
    public Dropdown armCorp; //type for now
    public Dropdown mission; //string value
    public Dropdown targetProvince; //string value

    /*
     * New Aggrement Parameters
     */
    public Dropdown aggrement; //string value
    public Dropdown targetCountry; //string value
    public Dropdown endDate; //string value

    /*
     * Make Law Parameters
     */
    public Dropdown lawTitle; //string value
    public Dropdown startDate; //string value

    /*
     * Set Budget Parameters
     */
    public Dropdown province; //string value
    public Dropdown budget; //int value

    
    private APIConnector apiConnector = null;



    void Start()
    {
        apiConnector = new APIConnector(this);
    }
    
    void Update()
    {
        
    }

    public void UserOptionChangeClicked(string btnType)
    {
        switch (btnType)
        {
            case "GiveMission":
                activeUserOptionChangePanel = giveMissionPanel;
                LoadMissionData();
                break;
            case "NewAggrement":
                activeUserOptionChangePanel = newAggrementPanel;
                LoadAggrementData();
                break;
            case "MakeLaw":
                activeUserOptionChangePanel = makeLawPanel;
                LoadLawData();
                break;
            case "SetBudget":
                activeUserOptionChangePanel = setBudgetPanel;
                LoadBudgetData();
                break;
        }

        transparentPanel.SetActive(true);
        activeUserOptionChangePanel.SetActive(true);
    }

    public void CloseUserOptionChangePanel()
    {
        activeUserOptionChangePanel.SetActive(false);
        transparentPanel.SetActive(false);
    }

    #region Data Loaders

    private void LoadMissionData()
    {
        //Remove all child objects from related dropdown
        armCorp.ClearOptions();
        mission.ClearOptions();
        targetProvince.ClearOptions();
        
        //Make request to fill table
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";
        
        //Add Corp Types
        apiConnector.makeRequest(AddCorps2List, "armyInformations", jsonToSend);
        armCorp.captionText.text = "Select Army Corp";

        //Add Mission Titles
        mission.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Move"));
        mission.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Attack"));
        mission.captionText.text = "Select Mission";

        //Add Targets
        foreach(Province province in InterchangableVars.provinces)
        {
            targetProvince.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(province.name));
        }
        targetProvince.captionText.text = "Select Target";

    }

    private void LoadAggrementData()
    {
        //Remove all child objects from related dropdown
        aggrement.ClearOptions();
        targetCountry.ClearOptions();
        endDate.ClearOptions();

        //Add Aggrements
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Declare War"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Cease-fire Aggrement"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Peace Aggrement"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Tax Payment Aggrement"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Open Borders For Army"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Weapon Aggrement"));
        aggrement.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Medic Tool Aggrement"));
        aggrement.captionText.text = "Select Aggrement";

        //Add Target Country
        foreach(Country country in InterchangableVars.countries.Values)
        {
            if (!country.isMyCountry)
            {
                targetCountry.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(country.name));
            }
        }
        targetCountry.captionText.text = "Select Target";

        //Add End Dates
        DateTime date = DateTime.Now.AddDays(1);
        endDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        date = DateTime.Now.AddDays(2);
        endDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        date = DateTime.Now.AddDays(3);
        endDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        endDate.captionText.text = "Select End Date";

    }

    private void LoadLawData()
    {
        //Remove all child objects from related dropdown
        lawTitle.ClearOptions();
        startDate.ClearOptions();
        
        //Add Laws
        lawTitle.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Medeni Kanun"));
        lawTitle.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("Tevhid-i Tedrisat"));
        lawTitle.captionText.text = "Select Law";

        //Add Start Dates
        DateTime date = DateTime.Now.AddDays(1);
        startDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        date = DateTime.Now.AddDays(2);
        startDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        date = DateTime.Now.AddDays(3);
        startDate.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(date.ToShortDateString()));
        startDate.captionText.text = "Select Start Date";
        
    }
    
    private void LoadBudgetData()
    {
        //Remove all child objects from related dropdown
        province.ClearOptions();
        budget.ClearOptions();
        
        //Add Provinces
        foreach(Province provinceX in InterchangableVars.provinces)
        {
            if (InterchangableVars.countries[provinceX.countryID].isMyCountry)
            {
                province.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(provinceX.name));
            }
        }
        province.captionText.text = "Select Province";

        //Add Budgets
        budget.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("10000"));
        budget.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("20000"));
        budget.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData("30000"));
        budget.captionText.text = "Select Budget";

    }
    #endregion

    #region Dropdown Item Adders
    
    private void AddCorps2List(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            Debug.Log(details.IsArray.ToString());

            //1 Result
            if (details.IsObject)
            {
                Debug.Log("AddCorp2List: " + details.ToString());
                armCorp.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(details["corpType"]));
            }
            //Multiple Results
            else
            {
                foreach (JSONNode unit in details)
                {
                    Debug.Log("AddCorp2List: " + unit.ToString());
                    armCorp.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(unit["corpType"]));
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
            Debug.Log(result["details"].ToString());
        }
    }
    
    #endregion

    //Applying Change
    public void UserOptionChangeApplied()
    {
        switch (activeUserOptionChangePanel.name)
        {
            case "GiveMissionPanel":
                ApplyGiveMission();
                break;
            case "NewAggrementPanel":
                ApplyNewAggrement();
                break;
            case "MakeLawPanel":
                ApplyMakeLaw();
                break;
            case "SetBudgetPanel":
                ApplySetBudget();
                break;
        }
    }

    #region User Option Appliers

    private void ApplyGiveMission()
    {
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "', 'corpType':'" + armCorp.captionText.text + "', 'provinceName':'" + targetProvince.captionText.text + "', 'mission':'" + mission.captionText.text + "'}";
        
        apiConnector.makeRequest(ApplyCheck, "giveMissionToCorps", jsonToSend);
    }
    
    private void ApplyNewAggrement()
    {
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "', 'c1Name':'" + InterchangableVars.myCountryName + "', 'c2Name':'" + targetCountry.captionText.text + "', 'aggrementName':'" + aggrement.captionText.text + "', 'endDate':'" + endDate.captionText.text + "'}";

        apiConnector.makeRequest(ApplyCheck, "offerAggrement", jsonToSend);
    }

    private void ApplyMakeLaw()
    {
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "', 'cName':'" + InterchangableVars.myCountryName + "', 'lawTitle':'" + lawTitle.captionText.text + "', 'startDate':'" + startDate.captionText.text + "'}";

        apiConnector.makeRequest(ApplyCheck, "makeLaw", jsonToSend);
    }

    private void ApplySetBudget()
    {
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "', 'provinceName':'" + province.captionText.text + "', 'year':'" + DateTime.Now.Year.ToString() + "', 'amount':'" + budget.captionText.text + "'}";

        apiConnector.makeRequest(ApplyCheck, "setBudgetForProvince", jsonToSend);
    }

    private void ApplyCheck(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //1 Result
            if (details.IsObject)
            {
                if (details["Result"] == 1)
                {
                    Debug.Log("Assignment is done");
                }
                else
                {
                    Debug.Log("Query didn't work!");
                }
            }
            //Multiple Results
            else
            {
                foreach (JSONNode unit in details)
                {
                    if (unit["Result"] == 1)
                    {
                        Debug.Log("Mission is assigned");
                    }
                    else
                    {
                        Debug.Log("Query didn't work!");
                    }
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
            Debug.Log(result["details"].ToString());
        }
    }

    #endregion
}
