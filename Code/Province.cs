using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Province {

    public int id;
    public string name;
    public string governorName;
    public int population;
    public int taxRate;
    public int countryID;

    public Province(int id, string name, string governorName, int population, int taxRate, int countryID)
    {
        this.id = id;
        this.name = name;
        this.governorName = governorName;
        this.population = population;
        this.taxRate = taxRate;
        this.countryID = countryID;
    }

}
