using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Utility
{
    public static GameObject CreateLabel(float x, float y, string text_to_print, int font_size, Color text_color, Transform parent)
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

    public static GameObject CreateImage(ArmyCorpMission armyCorpMission, Transform parent)
    {
        GameObject UICorpObj = new GameObject();
        /*

        UICorpObj.transform.SetParent(parent);

        //TODO: Calculate Position of Image according to screen-provinces-duration-current time-inital position
        //Get startTime
        //Get nowTime
        //Find time diff
        TimeSpan timeDiff = DateTime.Now.Subtract(armyCorpMission.startTime);
        //Find Inital position, target position [according to screen]
        //Find position diff
        //Get duration
        //Calculate Complete Ratio = timeDiff / duration [in minutes]
        //Calculate Completed Distance = Complete Ratio * Position diff
        //Calculate position diff unit vector
        //Calculate New Position = position diff unit vector * Completed Distance + Initial Position


        RectTransform trans = UICorpObj.AddComponent<RectTransform>();
        trans.anchoredPosition = new Vector2(0, 0);
        

        ContentSizeFitter fitter = UICorpObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        */
        return UICorpObj;
    }

    public static void GetUserSessionData()
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
