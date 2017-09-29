using UnityEngine;

public class BatAI : MonoBehaviour {
    //Speed at which the AI moves
    public float aiSpeed;
    public static bool GoHitBall;
    public GameObject ball;

    private Rigidbody rb;
    private bool ballHit;

    void Start () {
        rb = GetComponent<Rigidbody>();
        aiSpeed = 75;
        ballHit = false;
        GoHitBall = false;
    }

    // Update is called once per frame
    void Update () {
        if (ballHit)
        {
            if (!AIColliderScript.easyMode)
            {
                rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 150f), aiSpeed * Time.deltaTime);
            }
            else
            {
                rb.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 4.5f, 150f), aiSpeed * Time.deltaTime);
            }

            if (rb.position.z >= 150)
            {
                ballHit = false;
                aiSpeed = 75;
            }
        }
        else
        {
            if (GoHitBall)
            {
                HitBall(ball);
            }
        }
    }

    private void HitBall(GameObject ballGameObject)
    {
        if(!ballHit)
        {
            rb.position = Vector3.MoveTowards(transform.position,
                        ballGameObject.transform.position,
                        aiSpeed * 2.5f * Time.deltaTime);

            if (rb.position.magnitude - 50 <= ballGameObject.transform.position.magnitude)
            {
                rb.position = Vector3.MoveTowards(transform.position,
                ballGameObject.transform.position,
                aiSpeed * 15f * Time.deltaTime);

                if (rb.position.magnitude - 3 <= ballGameObject.transform.position.magnitude)
                {
                    GoHitBall = false;
                    ballHit = true;
                }
            }
        }
        else
        {
            ballHit = true;
            aiSpeed = 0.5f;
        }

    }

}
