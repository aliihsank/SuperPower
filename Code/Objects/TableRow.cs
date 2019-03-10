using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableRow : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void SetColumnValues(string[] vals)
    {
        for(int i=0; i < vals.Length; i++)
        {
            this.transform.GetChild(i).gameObject.GetComponent<Text>().text = vals[i];
        }
    }

    public void SetColumnNamesRow(string[] vals)
    {
        for (int i = 0; i < vals.Length; i++)
        {
            this.transform.GetChild(i).gameObject.GetComponent<Text>().text = vals[i];
            this.transform.GetChild(i).gameObject.GetComponent<Text>().fontStyle = FontStyle.Bold;
        }
    }

}
