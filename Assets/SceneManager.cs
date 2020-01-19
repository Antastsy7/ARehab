using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public Camera camera;
    private float wait = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        var pos = camera.transform.position;
        if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(0, 1.5f))<0.5f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }
    }
}
