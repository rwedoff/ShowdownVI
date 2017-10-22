using UnityEngine;
using UnityEngine.UI;

public class AIColliderScript : MonoBehaviour {
    public Dropdown levelDropDown;
    public static bool easyMode;
    private int missHitRate;
    private float oldTime;
    private bool timerStarted;
    private bool serveTimeStarted;

    private void Start()
    {
        missHitRate = 8;
        easyMode = true;

        timerStarted = false;
        oldTime = 0;

        if (levelDropDown != null)
        {
            levelDropDown.onValueChanged.AddListener(delegate
            {
                OnMyValueChange(levelDropDown);
            });
            levelDropDown.value = PlayerPrefs.GetInt("diff");
        }
    }

    private void OnMyValueChange(Dropdown dropDown)
    {
        easyMode = false;
        PlayerPrefs.SetInt("diff", dropDown.value);
        if (dropDown.value == 0) //Easy
        {
            
            easyMode = true;
            missHitRate = 8;
        }
        else if (dropDown.value == 1) //Medium
        {
            easyMode = true;
            missHitRate = 17;
        }
        else if (dropDown.value == 2) //Hard
        {
            missHitRate = 25;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball" && !BallScript.ballStart)
        {
            if (Random.Range(0, missHitRate) > 2)
            {
                BatAI.GoHitBall();
            }
            timerStarted = true;
            oldTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Ball")
        {
            timerStarted = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Ball")
        {
            if (timerStarted && !BallScript.ballStart)
            {
                //If ball is still in zone after 2 seconds then hit
                if (Time.time > oldTime + 5)
                {
                    Debug.Log("Waited too long");
                    BatAI.GoHitBall();
                    timerStarted = false;
                }
            }
            else if (BallScript.ballStart)
            {
                //Initate serve
                StartServe();
            }
            else
            {
                timerStarted = true;
                oldTime = Time.time;
            }
        }
    }

    private void StartServe()
    {
        if (serveTimeStarted)
        {
            Debug.Log("Should start serving");
            if(Time.time > oldTime + 5)
            {
                Debug.Log("Served!");
                BatAI.GoHitBall();
                serveTimeStarted = false;
            }
        }
        else
        {
            serveTimeStarted = true;
            oldTime = Time.time;
        }
    }
}
