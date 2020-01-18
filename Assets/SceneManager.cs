using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    private float wait = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        wait -= Time.deltaTime;
        Debug.Log(wait);
        if (wait <= 0) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }
    }
}
