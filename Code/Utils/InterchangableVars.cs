﻿using System.Collections.Generic;

public class InterchangableVars
{
    public static bool menuIsOpen = false;
    public static string email;
    public static string password;

    public static string myCountryName;

    public static int screenWidth;
    public static int screenHeight;

    public static Dictionary<int, Country> countries = new Dictionary<int, Country>();
    public static List<Province> provinces = new List<Province>();
    public static List<ArmyCorpMission> armyCorpMissions = new List<ArmyCorpMission>();
    
    public static string updateStatus = "finished";
    
    //Info-UI Update Locks
    public static bool myCountryRdy = false;
    public static bool otherCountriesRdy = false;
    public static int provincesRequestCounter = 0;
    public static bool armyCorpMissionDetailsRdy = false;
}
