using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Country  {

    public int id;
    public string name;
    public int totalPopulation;
    public int avgTax;
    public int numOfProvinces;
    public int remaining;

    public Color countryColor;

    public bool isMyCountry;

    public Country(int id, string name, int totalPopulation, int avgTax, int numOfProvinces, int remaining, bool isMyCountry)
    {
        this.id = id;
        this.name = name;
        this.totalPopulation = totalPopulation;
        this.avgTax = avgTax;
        this.numOfProvinces = numOfProvinces;
        this.remaining = remaining;

        float randR = Random.Range(0.1f, .5f);
        float randG = Random.Range(0.1f, .5f);
        float randB = Random.Range(0.1f, .5f);

        this.isMyCountry = isMyCountry;

        countryColor = new Color(randR, randG, randB, 1);
    }
	
}
