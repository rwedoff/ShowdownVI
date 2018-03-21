using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
    public GameObject ourBall;
    public GameObject naiveBall;
    public InputField nameField;
    public Text ballAndPosText;
    public Button saveExpButton;
    public Dropdown ballSpeedDropdown;
    public GameObject batObj;
    public Canvas menuCanvas;
    public Button startExpButton;
    public Text clockText;

    public static float TableEdge { get; private set; }
    public static float CenterX { get; private set; }
    public static bool NaiveMode { private get; set; }

    private GameObject _currentBall;
    private Dictionary<int, BallPath> ballPositions= new Dictionary<int, BallPath>();
    private IEnumerator<int> _expList;
    private int _currBallNumber;
    private List<ExpData> expResults = new List<ExpData>();
    private int _currBallType;
    private int _currBallSpeed;
    private bool playerReady;
    private AudioSource batSound;
    private float oldTime;
    private bool timerStarted;
    private float maxDistance;
    private GameObject ballObject;
    private System.Timers.Timer clockTimer;

    private enum HitRes { miss = 0, hit = 1, perfectHit = 2 }
    private HitRes thisHitres;

    /// <summary>
    /// AudioSources
    /// 0: Clapping, 1-5: Good 6-8: Missed
    /// </summary>
    private AudioSource[] _audioSources;
    private bool newBallOk;
    private string clockString;
    private DateTime startTime;
    private bool canPressButton;

    private void Start()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
        GameUtils.ballSpeedPointsEnabled = false;
        _currBallNumber = -1;
        CreateBallPosition();
        ShuffleArray();
        saveExpButton.onClick.AddListener(FinishExp);
        startExpButton.onClick.AddListener(StartExp);
        _audioSources = GetComponents<AudioSource>();
        BallScript.GameInit = false;
        playerReady = false;
        batSound = batObj.GetComponents<AudioSource>()[0];
        batSound.mute = true;
        StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
        TableEdge = 0;
        CenterX = 0;
        canPressButton = true;
        clockTimer = new System.Timers.Timer(1000);
        clockTimer.Elapsed += ClockTimer_Elapsed;
    }

    private void ClockTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        TimeSpan diff = e.SignalTime - startTime;
        clockString = diff.Minutes + ":" + diff.Seconds;
    }

    private void StartExp()
    {
        clockTimer.Start();
        startTime = DateTime.Now;
        StartNextBall(HitRes.hit);
    }

    private void Update()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;

        clockText.text = clockString;

        if (NaiveMode)
        {
            ballObject = naiveBall;
        }
        else
        {
            ballObject = ourBall;
        }

        //Check ball state
        //DEBUG ONLY
        //if(true)
        //END DEBUG
        //Wait for player initalization
        if (!playerReady)
        {
            batSound.mute = true;
            if (JoyconController.ButtonPressed)
            {
                playerReady = true;
                TableEdge = BodySourceView.baseKinectPosition.Z;
                CenterX = BodySourceView.baseKinectPosition.X;
            }
        }
        else
        {
            Time.timeScale = 1;
            BallScript.GameInit = false;
            batSound.mute = false;
        }

        //Perfect hit, start new ball
        if((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce) && maxDistance > 10)
        {
            timerStarted = false;
            BallScript.BallHitOnce = false;
            NaiveBallScript.BallHitOnce = false;
            //StartNextBall(true);
            StartCoroutine(PerfectHitStartNextBall());
            return;
        }

        //Ball Falls in goal, start new ball
        if (GoalScript.ExpBallLose)
        {
            GoalScript.ExpBallLose = false;
            StartNextBall(HitRes.miss);
        }

        //Wait for result of hit
        if (timerStarted)
        {
            if (BallScript.BallHitOnce || NaiveBallScript.BallHitOnce)
            {
                if(_currentBall != null)
                {
                    if (maxDistance <= _currentBall.transform.position.z)
                    {
                        maxDistance = _currentBall.transform.position.z;
                    }
                }
                else
                {
                    return;
                }
                
            }
            else
            {
                maxDistance = -130;
            }
            if(Time.time > oldTime + 8)
            {
                oldTime = Time.time;
                StartNextBall(DetermineHit(_currentBall));
            }
        }
    }

    private IEnumerator PerfectHitStartNextBall()
    {
        yield return new WaitForSeconds(1);
        StartNextBall(HitRes.perfectHit);
    }

    private HitRes DetermineHit(GameObject ball)
    {
        if((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce) && maxDistance > -50)
        {
            return HitRes.hit;
        }
        return HitRes.miss;
    }

    private void SpawnBall()
    {
        _currBallType = _expList.Current;
        _currBallNumber++;
        ballAndPosText.text = "Ball: " + _currBallNumber + "   Position: " + _currBallType;

        bool isNewBallAvail = _expList.MoveNext();
        if (!isNewBallAvail)
        {
            FinishExp();
            return;
        }

        _currentBall = Instantiate(ballObject, ballPositions[_currBallType].Origin, new Quaternion());
        Rigidbody rb = _currentBall.GetComponent<Rigidbody>();

        if(ballSpeedDropdown.value == 0) //Slow
        {
            _currBallSpeed = 75;
        }
        else if(ballSpeedDropdown.value == 1) //Medium
        {
            _currBallSpeed = 125;
        }
        else if (ballSpeedDropdown.value == 2) //Fast
        {
            _currBallSpeed = 250;
        }
        else if(ballSpeedDropdown.value == 3) //Random
        {
            _currBallSpeed = UnityEngine.Random.Range(75, 250);
        }

        //Play Click sound
        _currentBall.GetComponents<AudioSource>()[1].Play();

        rb.AddForce(ballPositions[_currBallType].Destination * _currBallSpeed, ForceMode.Acceleration);
    }

    private void CreateBallPosition()
    {
        //Left Start
        ballPositions.Add(0, new BallPath
        {
            Origin = new Vector3(-42, 5, 110),
            Destination = new Vector3(0, 5, -110)
        });
        ballPositions.Add(1, new BallPath
        {
            Origin = new Vector3(-42, 5, 110),
            Destination = new Vector3(11, 5, -110)
        });
        ballPositions.Add(2, new BallPath
        {
            Origin = new Vector3(-42, 5, 110),
            Destination = new Vector3(21, 5, -110)
        });
        ballPositions.Add(3, new BallPath
        {
            Origin = new Vector3(-42, 5, 110),
            Destination = new Vector3(31, 5, -110)
        });
        ballPositions.Add(4, new BallPath
        {
            Origin = new Vector3(-42, 5, 110),
            Destination = new Vector3(42, 5, -110)
        });

        //Middle Start
        ballPositions.Add(5, new BallPath
        {
            Origin = new Vector3(0, 5, 110),
            Destination = new Vector3(-21, 5, -110)
        });
        ballPositions.Add(6, new BallPath
        {
            Origin = new Vector3(0, 5, 110),
            Destination = new Vector3(-11, 5, -110)
        });
        ballPositions.Add(7, new BallPath
        {
            Origin = new Vector3(0, 5, 110),
            Destination = new Vector3(0, 5, -110)
        });
        ballPositions.Add(8, new BallPath
        {
            Origin = new Vector3(0, 5, 110),
            Destination = new Vector3(11, 5, -110)
        });
        ballPositions.Add(9, new BallPath
        {
            Origin = new Vector3(0, 5, 110),
            Destination = new Vector3(21, 5, -110)
        });

        //Right Start
        ballPositions.Add(14, new BallPath
        {
            Origin = new Vector3(42, 5, 110),
            Destination = new Vector3(0, 5, -110)
        });
        ballPositions.Add(13, new BallPath
        {
            Origin = new Vector3(42, 5, 110),
            Destination = new Vector3(-11, 5, -110)
        });
        ballPositions.Add(12, new BallPath
        {
            Origin = new Vector3(42, 5, 110),
            Destination = new Vector3(-21, 5, -110)
        });
        ballPositions.Add(11, new BallPath
        {
            Origin = new Vector3(42, 5, 110),
            Destination = new Vector3(-31, 5, -110)
        });
        ballPositions.Add(10, new BallPath
        {
            Origin = new Vector3(42, 5, 110),
            Destination = new Vector3(-42, 5, -110)
        });
    }

    private void StartNextBall(HitRes hitres)
    {
        //DEBUG ONLY
        //if (true)
        //END DEBUG
        if (playerReady)
        {
            if (newBallOk)
            {
                newBallOk = false;
                Debug.Log("Sending Ball");
                if (_currBallNumber != -1)
                    CollectExpData(hitres);
                Destroy(_currentBall);
                if (_currBallNumber != -1)
                {
                    StartCoroutine((hitres == HitRes.hit || hitres == HitRes.perfectHit) ? NextBallHit() : NextBallMissed());
                }
                else
                {
                    StartCoroutine(NextBallComing());
                }
                newBallOk = true;
            }
        }
        else
        {
            Debug.LogWarning("Player Not Ready: Need to press trigger");
        }

    }
    
    private void CollectExpData(HitRes hit)
    {
        expResults.Add(new ExpData()
        {
            ParticipantId = nameField.text,
            BallNumber = _currBallNumber,
            BallType = _currBallType,
            BallSpeed = _currBallSpeed,
            BallResult = (int) hit
        });
    }

    private void FinishExp()
    {
        if (canPressButton)
        {
            canPressButton = false;
            StartCoroutine(SaveCSV());
        }
    }

    private IEnumerator SaveCSV()
    {
        NumberSpeech.PlayAudio("thanks");
        yield return new WaitForSeconds(2);
        AudioListener.pause = true;
        int option = EditorUtility.DisplayDialogComplex("Finish Experiment",
            "Are you sure you want to finish this experiment?",
            "Save",
            "Cancel",
            "Quit Without Saving");
        switch (option)
        {
            // Save
            case 0:
                Destroy(_currentBall);
                CreateFile();
                menuCanvas.enabled = true;
                ResetExp();
                clockTimer.Stop();
                break;

            // Cancel.
            case 1:
                break;

            // Quit Without saving.
            case 2:
                Destroy(_currentBall);
                menuCanvas.enabled = true;
                clockTimer.Stop();
                ResetExp();
                break;

            default:
                Debug.LogError("Unrecognized option.");
                break;
        }
        AudioListener.pause = false;
        canPressButton = true;
    }

    private void CreateFile()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Participant_Id" + "," + "Ball_Number" + "," + "Ball_Type" + ", " + "Ball_Speed" + "," + "Ball_Result");
        foreach (var data in expResults)
        {
            sb.AppendLine(data.ToString());
        }
        string naiveModeStr = NaiveMode ? "_navie_" : "_our_";
        
        var path = EditorUtility.SaveFilePanel(
              "Save Experiment as CSV",
              "",
              nameField.text + naiveModeStr + "exp.csv",
              "csv");

        if (path.Length != 0)
        {
            File.WriteAllBytes(path, new UTF8Encoding().GetBytes(sb.ToString()));
        }

    }

    private IEnumerator NextBallHit()
    {
        _audioSources[0].Play();
        _audioSources[UnityEngine.Random.Range(1, 5)].Play();
        yield return new WaitForSeconds(_audioSources[0].clip.length);
        yield return NextBallComing();
    }

    private IEnumerator NextBallMissed()
    {
        int rand = UnityEngine.Random.Range(6, 8);
        _audioSources[rand].Play();
        yield return new WaitForSeconds(_audioSources[rand].clip.length);
        yield return NextBallComing();
    }

    private IEnumerator NextBallComing()
    {
        AudioSource aud = NumberSpeech.PlayAudio("nextball");
        yield return new WaitForSeconds(aud.clip.length + 0.2f);
        SpawnBall();
        timerStarted = true;
        oldTime = Time.time;
    }

    private void ShuffleArray()
    {
        List<int> expListLoc = new List<int>();
        for (int i = 0; i <= 14; i++) //Range of positions
        {
            for (int j = 0; j < 2; j++) //Times per position
            {
                expListLoc.Add(i);
            }
        }
        System.Random rng = new System.Random();
        int n = expListLoc.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = expListLoc[k];
            expListLoc[k] = expListLoc[n];
            expListLoc[n] = value;
        }
        _expList = expListLoc.GetEnumerator();
    }

    private void ResetExp()
    {
        _expList.Reset();
        _currBallNumber = -1;
        GameUtils.ballSpeedPointsEnabled = false;
        BallScript.GameInit = false;
        playerReady = false;
        batSound = batObj.GetComponents<AudioSource>()[0];
        batSound.mute = true;
        StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
        expResults.Clear();
    }
}

public class ExpData
{
    public string ParticipantId { get; set; }
    public int BallNumber { get; set; }
    public int BallType { get; set; }
    public int BallSpeed { get; set; }
    public int BallResult { get; set; }

    public override string ToString()
    {
        return ParticipantId + "," + BallNumber + "," + BallType + "," + BallSpeed + "," + BallResult;
    }
}

public class BallPath
{
    public Vector3 Origin { get; set; }
    public Vector3 Destination { get; set; }
}
