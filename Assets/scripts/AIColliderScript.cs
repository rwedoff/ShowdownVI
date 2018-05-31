using UnityEngine;
using UnityEngine.UI;

public class AIColliderScript : MonoBehaviour {
    public Dropdown levelDropDown;
    internal static bool ballInZone;
    internal static int difficulty;

    private void Start()
    {
        BatAI.ballHitSpeed = 50;
        BatAI.aiSpeed = 75;
        difficulty = PlayerPrefs.GetInt("diff");
    }

    public static void SaveDifficulty(int pref)
    {
        if (pref == 0) //Easy
        {
            BatAI.aiSpeed = 150;
            BatAI.ballHitSpeed = 75;
        }
        else if (pref == 1) //Medium
        {
            BatAI.aiSpeed = 200;
            BatAI.ballHitSpeed = 90;
        }
        else if (pref == 2) //Hard
        {
            BatAI.aiSpeed = 300;
            BatAI.ballHitSpeed = 150;
        }
        PlayerPrefs.SetInt("diff", pref);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Ball")
        {
            ballInZone = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Ball")
        {
            ballInZone = true;
        }
    }
}
