using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoUpdater : MonoBehaviour
{
    private APIConnector apiConnector = null;

    private bool updateStarted = false;

    void Start()
    {
        apiConnector = new APIConnector(this);
        
    }
    
    void Update()
    {
        if (!updateStarted && InterchangableVars.email != "" && InterchangableVars.password != "")
        {
            updateStarted = true;
            StartCoroutine(StartUpdateInfos());
        }
    }

    public IEnumerator StartUpdateInfos()
    {
        string jsonToSend = @"{'email':'" + InterchangableVars.email + "', 'password':'" + InterchangableVars.password + "'}";
        
        while (true)
        {
            if (InterchangableVars.updateStatus == "finished")
            {
                InterchangableVars.updateStatus = "started";
                //Debug.Log("Update Started!");

                //Reset existing lists
                InterchangableVars.countries = new Dictionary<int, Country>();
                InterchangableVars.provinces = new List<Province>();

                //Get New Values and Set them to lists
                apiConnector.makeRequest(AddCountries, "myCountryDetails", jsonToSend);
                apiConnector.makeRequest(AddCountries, "otherCountriesDetails", jsonToSend);
                apiConnector.makeRequest(AddProvinces, "myProvincesDetails", jsonToSend);
                apiConnector.makeRequest(AddProvinces, "otherProvincesDetails", jsonToSend);
            }
            yield return new WaitForSeconds(60);
        }
    }

    public void AddCountries(string request, JSONNode result)
    {
        if (result["info"] == 1)
        {
            JSONNode details = result["details"];

            //Only 1 country
            if (details.IsObject)
            {
                Debug.Log("AddMyCountry: " + details.ToString());
                InterchangableVars.countries.Add(details["id"], new Country(details["id"], details["cname"], details["totalpopulation"], details["avgTax"], details["numOfProvinces"], details["remaining"], true));
                InterchangableVars.myCountryRdy = true;
            }
            //List of Countries
            else
            {
                foreach (JSONNode country in details)
                {
                    Debug.Log("AddOtherCountries: " + country.ToString());
                    InterchangableVars.countries.Add(country["id"], new Country(country["id"], country["cname"], country["totalpopulation"], country["avgTax"], country["numOfProvinces"], country["remaining"], false));
                }
                InterchangableVars.otherCountriesRdy = true;
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
                InterchangableVars.provinces.Add(new Province(details["id"], details["pname"], details["governorName"], details["population"], details["taxRate"], details["countryID"]));
            }
            //List of Provinces
            else
            {
                foreach (JSONNode province in details)
                {
                    Debug.Log("AddProvinces: " + province.ToString());
                    InterchangableVars.provinces.Add(new Province(province["id"], province["pname"], province["governorName"], province["population"], province["taxRate"], province["countryID"]));
                }
            }
            InterchangableVars.provincesRequestCounter++;
        }
        else
        {
            Debug.Log("Request is unsuccesful!");
        }
    }

}
