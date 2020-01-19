using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GameObject city;
    public GameObject medal;
    public AudioSource level1, level3, success;
    private int destroyed, killed;
    private bool back;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (back && time >= 20.0f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title", LoadSceneMode.Single);
        }
    }

    public void DestoryUFO()
    {
        
        destroyed += 1;
        Debug.Log(destroyed);
        if (destroyed >= 4)
        {
            Showcity();
        }
    }

    public void Showcity()
    {
        city.SetActive(true);
        level1.Stop();
        level3.Play();
    }

    public void KilledAlien()
    {
        killed += 1;
        Debug.Log("Killed");
        if (killed >= 4)
        {
            level3.Stop();
            success.Play();
            medal.SetActive(true);
            medal.GetComponentInChildren<TextMeshPro>().text = "Your saved our life in " + time.ToString("0.0")
            + "s";
            time = 0;
            back = true;
        }
    }
    
}
