using UnityEngine;
using UnityEngine.UI;

public class AIColliderScript : MonoBehaviour {
    public Dropdown levelDropDown;
    public static bool easyMode;
    private int missHitRate;

    private void Start()
    {
        missHitRate = 8;
        easyMode = true;

        levelDropDown.onValueChanged.AddListener(delegate {
            OnMyValueChange(levelDropDown);
        });
    }

    private void OnMyValueChange(Dropdown dropDown)
    {
        easyMode = false;
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
        if(other.tag == "Ball")
        {
            if (UnityEngine.Random.Range(0, missHitRate) > 2)
            {
                BatAI.GoHitBall = true;
            }
        }
    }
}
