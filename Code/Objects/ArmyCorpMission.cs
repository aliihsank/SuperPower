using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyCorpMission
{
    public string corpType;
    public int numOfSoldier;
    public string from;
    public string to;
    public string mission;
    public DateTime startTime;
    public int duration;

    public ArmyCorpMission(string corpType, int numOfSoldier, string from, string to, string mission, DateTime startTime, int duration)
    {
        this.corpType = corpType;
        this.numOfSoldier = numOfSoldier;
        this.from = from;
        this.to = to;
        this.mission = mission;
        this.startTime = startTime;
        this.duration = duration;
    }
}
