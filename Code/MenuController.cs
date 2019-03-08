using SimpleJSON;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject militaryPanel;
    public GameObject diplomacyPanel;
    public GameObject lawsPanel;
    public GameObject budgetPanel;

    private GameObject activeUserOptionPanel;

    public GameObject militaryTablePanel;
    public GameObject diplomacyTablePanel;
    public GameObject lawsTablePanel;
    public GameObject budgetTablePanel;

    public GameObject tableRowPrefab;
    
    public GameObject countryLabelsGroup;
    public GameObject provinceLabelsGroup;

    public Text dateTimeTextObj;

    private APIConnector apiConnector = null;
    private int activeMap = 0; //Country Map(default), 1=Provinces Map


    void Start()
    {
        apiConnector = new APIConnector(this);

        StartCoroutine(UpdateTime());
    }


    void Update()
    {
        
    }

    #region User Option Menus

    public void UserOptionClicked(string btnType)
    {
        InterchangableVars.menuIsOpen = true;

        switch (btnType)
        {
            case "Settings":
                activeUserOptionPanel = settingsPanel;
                break;
            case "Military":
                activeUserOptionPanel = militaryPanel;
                LoadMilitaryTable();
                break;
            case "Diplomacy":
                activeUserOptionPanel = diplomacyPanel;
                LoadDiplomacyTable();
                break;
            case "Law":
                activeUserOptionPanel = lawsPanel;
                LoadLawsTable();
                break;
            case "Budget":
                activeUserOptionPanel = budgetPanel;
                LoadBudgetTable();
                break;
        }
        activeUserOptionPanel.SetActive(true);
    }
    
    public void LoadMilitaryTable()
    {
        //Remove all child objects from militaryTablePanel
        for (int i = 0; i < militaryTablePanel.transform.childCount; i++)
        {
            Destroy(militaryTablePanel.transform.GetChild(i).gameObject);
        }

        //Add Column names row
        var columnNamesRow = Instantiate(tableRowPrefab, militaryTablePanel.transform, false);
        columnNamesRow.GetComponent<TableRow>().SetColumnNamesRow(new string[] { "Type", "Soldier Number", "Mission", "Where" });
        
        //Make request to fill table
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";

        apiConnector.makeRequest(AddMilitaryUnit2List, "armyInformations", jsonToSend);

    }

    public void LoadDiplomacyTable()
    {
        //Remove all child objects from diplomacyTablePanel
        for (int i = 0; i < diplomacyTablePanel.transform.childCount; i++)
        {
            Destroy(diplomacyTablePanel.transform.GetChild(i).gameObject);
        }

        //Add Column names row
        var columnNamesRow = Instantiate(tableRowPrefab, diplomacyTablePanel.transform, false);
        columnNamesRow.GetComponent<TableRow>().SetColumnNamesRow(new string[] { "Country-1", "Country-2", "Aggrement", "End Date" });
        
        //Make request to fill table
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";

        apiConnector.makeRequest(AddAggrement2List, "aggrementsInformations", jsonToSend);
    }

    public void LoadLawsTable()
    {
        //Remove all child objects from diplomacyTablePanel
        for (int i = 0; i < lawsTablePanel.transform.childCount; i++)
        {
            Destroy(lawsTablePanel.transform.GetChild(i).gameObject);
        }

        //Add Column names row
        var columnNamesRow = Instantiate(tableRowPrefab, lawsTablePanel.transform, false);
        columnNamesRow.GetComponent<TableRow>().SetColumnNamesRow(new string[] { "Title", "Content", "Start Date", "End Date" });
        
        //Make request to fill table
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";

        apiConnector.makeRequest(AddLaw2List, "lawsInformations", jsonToSend);
    }

    public void LoadBudgetTable()
    {
        //Remove all child objects from diplomacyTablePanel
        for (int i = 0; i < budgetTablePanel.transform.childCount; i++)
        {
            Destroy(budgetTablePanel.transform.GetChild(i).gameObject);
        }

        //Add Column names row
        var columnNamesRow = Instantiate(tableRowPrefab, budgetTablePanel.transform, false);
        columnNamesRow.GetComponent<TableRow>().SetColumnNamesRow(new string[] { "Province", "Amount", "Year" });
        
        //Make request to fill table
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";

        apiConnector.makeRequest(AddBudget2List, "budgetInformations", jsonToSend);
    }
    
    public void AddMilitaryUnit2List(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //1 Result
            if (details.IsObject)
            {
                Debug.Log("AddMilitaryUnit: " + details.ToString());

                var unitRow = Instantiate(tableRowPrefab, militaryTablePanel.transform, false);
                unitRow.GetComponent<TableRow>().SetColumnValues(new string[] { details["corpType"], details["numOfSoldiers"], details["mission"], details["pname"] });
            }
            //Multiple Results
            else
            {
                foreach (JSONNode unit in details)
                {
                    Debug.Log("AddMilitaryUnits: " + unit.ToString());

                    var unitRow = Instantiate(tableRowPrefab, militaryTablePanel.transform, false);
                    unitRow.GetComponent<TableRow>().SetColumnValues(new string[] { unit["corpType"], unit["numOfSoldiers"], unit["mission"], unit["pname"] });
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
            Debug.Log(result["details"].ToString());
        }
    }
    
    public void AddAggrement2List(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //1 Result
            if (details.IsObject)
            {
                Debug.Log("AddSingleAggrement: " + details.ToString());

                var aggrementRow = Instantiate(tableRowPrefab, diplomacyTablePanel.transform, false);
                aggrementRow.GetComponent<TableRow>().SetColumnValues(new string[] { details["cname1"], details["cname2"], details["aggrementType"], details["endDate"] });
            }
            //Multiple Results
            else
            {
                foreach (JSONNode aggrement in details)
                {
                    Debug.Log("AddMultipleAggrements: " + aggrement.ToString());

                    var aggrementRow = Instantiate(tableRowPrefab, diplomacyTablePanel.transform, false);
                    aggrementRow.GetComponent<TableRow>().SetColumnValues(new string[] { aggrement["cname1"], aggrement["cname2"], aggrement["aggrementType"], aggrement["endDate"] });
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
            Debug.Log("Info: " + result["info"] + "    Details:" + result["details"]);
        }
    }
    
    public void AddLaw2List(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //1 Result
            if (details.IsObject)
            {
                Debug.Log("AddSingleLaw: " + details.ToString());

                var lawRow = Instantiate(tableRowPrefab, lawsTablePanel.transform, false);
                lawRow.GetComponent<TableRow>().SetColumnValues(new string[] { details["title"], details["content"], details["startDate"], details["endDate"] });
            }
            //Multiple Results
            else
            {
                foreach (JSONNode law in details)
                {
                    Debug.Log("AddMultipleLaws: " + law.ToString());

                    var lawRow = Instantiate(tableRowPrefab, lawsTablePanel.transform, false);
                    lawRow.GetComponent<TableRow>().SetColumnValues(new string[] { law["title"], law["content"], law["startDate"], law["endDate"] });
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
            Debug.Log("Info: " + result["info"] + "    Details:" + result["details"]);
        }
    }
    
    public void AddBudget2List(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //1 Result
            if (details.IsObject)
            {
                Debug.Log("AddSingleBudget: " + details.ToString());

                var budgetRow = Instantiate(tableRowPrefab, budgetTablePanel.transform, false);
                budgetRow.GetComponent<TableRow>().SetColumnValues(new string[] { details["pname"], details["amount"], details["year"] });
            }
            //Multiple Results
            else
            {
                foreach (JSONNode budget in details)
                {
                    Debug.Log("AddMultipleBudgets: " + budget.ToString());

                    var budgetRow = Instantiate(tableRowPrefab, budgetTablePanel.transform, false);
                    budgetRow.GetComponent<TableRow>().SetColumnValues(new string[] { budget["pname"], budget["amount"], budget["year"] });
                }
            }
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
        }
    }


    public void CloseUserOptionPanel()
    {
        activeUserOptionPanel.SetActive(false);
        InterchangableVars.menuIsOpen = false;
    }

    #endregion


    #region Change Map Menu

    public void ChangeMap()
    {
        if (activeMap == 0)
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

    #endregion

    #region Menu Methods
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


    #endregion

}
