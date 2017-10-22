using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAI : MonoBehaviour {
    //Speed at which the AI moves
    public float aiSpeed;
    public static bool goHitBallBool;
    public GameObject ball;

    private Rigidbody rb;
    private bool ballHit;
    private bool batHome;

    void Start () {
        rb = GetComponent<Rigidbody>();
        aiSpeed = 75;
        ballHit = false;
        goHitBallBool = false;
        batHome = true;
    }

    // Update is called once per frame
    void Update () {



        //if (BallScript.ballStart && !batHome)
        //{
        //    rb.MovePosition(new Vector3(0, 4.5f, 150));
        //    batHome = true;
        //    Debug.Log("HERE");
        //    StartCoroutine(PauseGameTemp());
        //}

        //if (BallScript.ballStart && !GameUtils.playerServe)
        //{
        //    Debug.Log("WAIT");
            
        //}

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
            if (goHitBallBool)
            {
                HitBall(ball);
            }
        }
    }

    private IEnumerator PauseGameTemp()
    {
        Debug.Log("Before wait");
        yield return new WaitForSeconds(3);
        HitBall(GameObject.FindGameObjectWithTag("Ball"));
        Debug.Log("after wait");
    }

    private void HitBall(GameObject ballGameObject)
    {
        if(!ballHit)
        {
            rb.position = Vector3.MoveTowards(transform.position,
                        ballGameObject.transform.position,
                        aiSpeed * 2.5f * Time.deltaTime);
            if (rb.position.magnitude <= ballGameObject.transform.position.magnitude)
            {
                rb.MoveRotation(Quaternion.Euler(new Vector3(0, ballGameObject.transform.position.x, 0)));
                Vector3 hitPos = ballGameObject.transform.position;
                rb.MovePosition(new Vector3(hitPos.x, hitPos.y, hitPos.z - Random.Range(0.5f, 1.5f)));
                goHitBallBool = false;
                ballHit = true;
            }
        }
        else
        {
            ballHit = true;
            aiSpeed = 0.5f;
        }
        batHome = false;
    }

    public static void GoHitBall()
    {
        goHitBallBool = true;
    }

}
