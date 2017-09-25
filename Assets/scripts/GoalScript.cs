using UnityEngine;
using UnityEngine.UI;

public class GoalScript : MonoBehaviour {
    public Text southScoreText;
    public Text northScoreText;
    private int southScore;
    private int northScore;
    private AudioSource winPointAudio;
    private AudioSource losePointAudio;

    private void Start()
    {
        southScore = 0;
        northScore = 0;
        winPointAudio = transform.parent.GetComponents<AudioSource>()[0];
        losePointAudio = transform.parent.GetComponents<AudioSource>()[1];
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball") {
            if (gameObject.tag == "SouthGoal")
            {
                northScore++;
                northScoreText.text = "North Score: " + northScore;
                losePointAudio.Play();

            }
            else if (gameObject.tag == "NorthGoal")
            {
                southScore++;
                southScoreText.text = "South Score: " + southScore;
                winPointAudio.Play();
            }
            ResetBall(other.gameObject);
        }
    }

    private void ResetBall(GameObject ball)
    {
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0,0,0);
        rb.MovePosition(new Vector3(0, 3, -120f));
    }

}
