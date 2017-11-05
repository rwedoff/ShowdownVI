using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAI : MonoBehaviour {
    //Speed at which the AI moves
    public static float aiSpeed;
    public GameObject ball;
    private Rigidbody rb;
    private bool settingBallTriggered;

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
        settingBallTriggered = false;
        batAIState = BatAIState.hittingBall;
        //DEBUG?
        GameUtils.playState = GameUtils.GamePlayState.SettingBall;
        //hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
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
            hitPosition = hitPosition = GameObject.FindGameObjectWithTag("Ball").transform.position;
            //GameUtils.playState = GameUtils.GamePlayState.InPlay;
            //AIColliderScript.ballInZone = true;
            //batAIState = BatAIState.atHome;
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
            settingBallTriggered = false;
        }

    }

    private void GoHome()
    {
        rb.position = Vector3.MoveTowards(transform.position, new Vector3(0, 4.5f, 128f), aiSpeed / 1.2f * Time.deltaTime);
        homePosition = new Vector3(0, 4.5f, 128f);
        if(rb.position == homePosition)
        {
            batAIState = BatAIState.atHome;
        }
    }

    //private IEnumerator PauseGameTemp()
    //{
        
    //    if (!settingBallTriggered)
    //    {
    //        settingBallTriggered = true;
    //        AISetBall = false;
    //        yield return new WaitForSeconds(5); // Wait for score to be announced
    //        AISetBall = true;
    //        yield return new WaitForSeconds(6);
    //        settingBallTriggered = false;
    //    }
    //}


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
