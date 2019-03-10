using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(waitSeconds());
    }
    
    void Update()
    {
        
    }

    IEnumerator waitSeconds()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("LoginScene");
    }
}
