using System;
using UnityEngine;

public class BatAI : MonoBehaviour {
    //Speed at which the AI moves
    public static float aiSpeed;
    public GameObject ball;
    private Rigidbody rb;

    private Vector3 hitPosition;
    public enum BatAIState
    {
        atHitPosition, atHome, hittingBall
    }
    public BatAIState batAIState;

    private Vector3 homePosition;

    public static bool AISetBall { get; internal set; }

    public static float ballHitSpeed { get; internal set; }

    private Transform oppoGoalTransform;

    void Start () {
        rb = GetComponent<Rigidbody>();
        batAIState = BatAIState.hittingBall;
        GameUtils.playState = GameUtils.GamePlayState.SettingBall;
        oppoGoalTransform = GameObject.FindGameObjectWithTag("SouthGoal").transform;

    }

    // Update is called once per frame
    void Update () {
        //It is AI serve
        if (GameUtils.playState == GameUtils.GamePlayState.SettingBall)
        {
            GoHome();
        }
        else if (GameUtils.playState == GameUtils.GamePlayState.BallSet)
        {
            hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
        }
        else if (GameUtils.playState == GameUtils.GamePlayState.InPlay)
        {
            if (batAIState == BatAIState.hittingBall)
            {
                HitBall();
            }
            else if (batAIState == BatAIState.atHitPosition)
            {
                GoHome();
            }
            else if (batAIState == BatAIState.atHome && AIColliderScript.ballInZone)
            {
                hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
                batAIState = BatAIState.hittingBall;
            }
        }

    }

    private void GoHome()
    {
        if(AIColliderScript.difficulty == 0)
        {
            rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed / 1.5f * Time.deltaTime);
        }
        else if(AIColliderScript.difficulty == 1)
        {
            rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed / 1.2f * Time.deltaTime);
        }
        else
        {
            rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed * 1.5f * Time.deltaTime);
        }
        homePosition = new Vector3(0, 4.5f, 128f);
        if(rb.position == homePosition)
        {
            batAIState = BatAIState.atHome;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ball")
        {

            batAIState = BatAIState.atHitPosition;
            var inverseBallSpeed = Mathf.Min(Mathf.Max(Math.Abs(collision.rigidbody.velocity.z), ballHitSpeed), 100);

            if(collision.rigidbody.velocity.z > 0)
            {
                inverseBallSpeed *= -1;
            }

            var rbVelocityInverse = new Vector3(collision.rigidbody.velocity.x * -1, 0, inverseBallSpeed * 1.5f * -1);
            collision.rigidbody.velocity = rbVelocityInverse;
        }
    }

    private void HitBall()
    {
        float ballX = hitPosition.x,
              ballZ = hitPosition.z;
        //Debug.Log(ballX + " " + transform.position.x + " " +  ballZ + " " + transform.position.z);
        if (ballX != transform.position.x || ballZ != transform.position.z)
        {
        //Debug.Log(ballZ + " " + transform.position.z);
            rb.position = Vector3.MoveTowards(transform.position,
                        new Vector3(ballX, transform.position.y, ballZ),
                        aiSpeed * Time.deltaTime);
            transform.LookAt(oppoGoalTransform);
        }
        else
        {
            batAIState = BatAIState.atHitPosition;
        }
    }

}
