using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
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
    public GameObject globalSpeechGameObject;

    public static float TableEdge { get; private set; }
    public static float CenterX { get; private set; }
    public static bool NaiveMode { private get; set; }
    public static List<ExperimentLogFile> LogFileList = new List<ExperimentLogFile>();
    public static string globalClockString;
    public static string ParticipantId { private get; set; }

    private string clockString;
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
    private Timer clockTimer;
    private Timer globalTimer;
    private bool canPressStartButton;
    private int gamePoints;
    private enum HitRes { miss = 0, hitNotPastHalf = 1, pastHalfHit = 2, goal = 3 }
    private HitRes thisHitres;
    private NumberSpeech numberSpeech;

    /// <summary>
    /// AudioSources
    /// 0: Clapping, 1-8: Good 9-13: Missed
    /// </summary>
    private AudioSource[] _audioSources;
    private bool newBallOk;
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
        numberSpeech = globalSpeechGameObject.GetComponent<NumberSpeech>();
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
        clockTimer = new Timer(100);
        clockTimer.Elapsed += ClockTimer_Elapsed;
        globalTimer = new Timer(100);
        globalTimer.Elapsed += GlobalTimer_Elapsed;
        globalTimer.Start();
        ExperimentLog.Log("Program Started", time: DateTime.Now.ToLongTimeString());
        gamePoints = 0;
        canPressStartButton = true;
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
            StartCoroutine(HitPastHalfStartNextBall());
            return;
        }

        //Ball Falls in goal, start new ball
        if (GoalScript.ExpBallLose)
        {
            GoalScript.ExpBallLose = false;
            StartNextBall(HitRes.miss);
        }
        if (GoalScript.ExpBallWin)
        {
            GoalScript.ExpBallWin = false;
            StartNextBall(HitRes.goal);
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

    private void GlobalTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        globalClockString = e.SignalTime.ToLongTimeString() + " +" + e.SignalTime.Millisecond; ;
    }

    private void ClockTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        TimeSpan diff = e.SignalTime - startTime;
        clockString = diff.Minutes + ":" + diff.Seconds + "." + diff.Milliseconds;
    }

    /// <summary>
    /// Click listener that starts the game. Must have player press a button and the researcher press start
    /// </summary>
    private void StartExp()
    {
        ExperimentLog.Log("Attempted to Start Experiment");
        if (canPressStartButton && playerReady)
        {
            canPressStartButton = false;
            clockTimer.Start();
            startTime = DateTime.Now;
            StartNextBall(HitRes.hitNotPastHalf); //Starting game, params don't matter here.
        }
    }

    /// <summary>
    /// Starts a new ball after waiting 1.5 seconds based on a perfect hit (a hit that went past the halfway point)
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitPastHalfStartNextBall()
    {
        yield return new WaitForSeconds(1.5f); //Time allowed once ball goes past halfway point
        StartNextBall(HitRes.pastHalfHit);
    }

    /// <summary>
    /// Determines if the ball was hit, and didn't go past the halfway or it was missed
    /// </summary>
    /// <param name="ball"></param>
    /// <returns>HitRes.miss or HitRes.hit</returns>
    private HitRes DetermineHit(GameObject ball)
    {
        if((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce) && maxDistance > -50)
        {
            return HitRes.hitNotPastHalf;
        }
        return HitRes.miss;
    }

    /// <summary>
    /// Spawns a new ball based on the 30 balls of the experiment list.
    /// </summary>
    private void SpawnBall()
    {
        if (!canPressStartButton && playerReady) //Double check to present sending two balls in transition
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

            if (ballSpeedDropdown.value == 0) //Slow
            {
                _currBallSpeed = 75;
            }
            else if (ballSpeedDropdown.value == 1) //Medium
            {
                _currBallSpeed = 125;
            }
            else if (ballSpeedDropdown.value == 2) //Fast
            {
                _currBallSpeed = 250;
            }
            else if (ballSpeedDropdown.value == 3) //Random
            {
                _currBallSpeed = UnityEngine.Random.Range(75, 250);
            }

            //Play Click sound
            _currentBall.GetComponents<AudioSource>()[1].Play();

            rb.AddForce(ballPositions[_currBallType].Destination * _currBallSpeed, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Creation of list of ball positions and their destinations.
    /// </summary>
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

    /// <summary>
    /// Starts the next ball and adds to the total gamePoints
    /// </summary>
    /// <param name="hitres"></param>
    /// <param name="pointsToAdd"></param>
    private void StartNextBall(HitRes hitres)
    {
        if (playerReady && !canPressStartButton)
        {
            if (newBallOk)
            {
                newBallOk = false;
                Debug.Log("Sending Ball");
                ExperimentLog.Log("Sending Ball");
                if (_currBallNumber != -1)
                {
                    gamePoints += (int)hitres; //The points correlate to the hitres
                    CollectExpData(hitres);
                }
                Destroy(_currentBall);
                if (_currBallNumber != -1)
                {
                    StartCoroutine(hitres != HitRes.miss ? NextBallHit() : NextBallMissed());
                }
                else
                {
                    StartCoroutine(NextBallComing());
                }
            }
        }
        else
        {
            Debug.LogWarning("Player Not Ready: Need to press trigger");
            ExperimentLog.Log("Player Not Ready", tag: "warn");
        }

    }

    /// <summary>
    /// Collects the data from a current hit session and creates a new ExpData object for the saved CSV
    /// </summary>
    /// <param name="hit"></param>
    private void CollectExpData(HitRes hit)
    {
        expResults.Add(new ExpData()
        {
            ParticipantId = nameField.text,
            BallNumber = _currBallNumber,
            BallType = _currBallType,
            BallSpeed = _currBallSpeed,
            BallResult = (int) hit,
            EventTime = globalClockString,
            TimerTime = clockString,
            GamePoints = gamePoints
        });
    }

    /// <summary>
    /// On click for the finish button in the UI.
    /// </summary>
    private void FinishExp()
    {
        if (canPressButton)
        {
            canPressButton = false;
            canPressStartButton = true;
            SaveCSV();
        }
    }

    /// <summary>
    /// Pulls up a GUI menu and based on selected option saves a CSV file with the data and log file.
    /// </summary>
    private void SaveCSV()
    {
        newBallOk = false;
        //NumberSpeech.PlayAudio("thanks");
        //yield return new WaitForSeconds(2);
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
                newBallOk = false;
                clockTimer.Stop();
                ResetExp();
                break;

            // Cancel.
            case 1:
                newBallOk = true;
                break;

            // Quit Without saving.
            case 2:
                Destroy(_currentBall);
                menuCanvas.enabled = true;
                clockTimer.Stop();
                newBallOk = false;
                ResetExp();
                break;

            default:
                Debug.LogError("Unrecognized option.");
                ExperimentLog.Log("Error in save menu", tag:"Error");
                break;
        }
        AudioListener.pause = false;
        canPressButton = true;
    }

    /// <summary>
    /// Creates the file to be saved and saves to disk
    /// </summary>
    private void CreateFile()
    {
        ExperimentLog.Log("Creating file");
        var sb = new StringBuilder();
        //Append Exp result headers
        sb.AppendLine("Event_Time" + "," + "Timer_Time" + "," + "Participant_Id" + ","
            + "Ball_Number" + "," + "Ball_Type" + ", " + "Ball_Speed" + "," + 
            "Ball_Result [Miss->0 : MaybeHit->1 : HitPastHalf->2 : Goal->3]" + "," + "Game_Points");
        //Append Exp results
        foreach (var data in expResults)
        {
            sb.AppendLine(data.ToString());
        }
        //Append Log Headers
        sb.AppendLine("\n\n");
        sb.AppendLine("Event_Time" + "," + "Tag" + "," + "Message");
        //Append Log Results
        foreach (var data in LogFileList)
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

    /// <summary>
    /// Audio for a hit ball and starts a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallHit()
    {
        _audioSources[0].Play();
        _audioSources[UnityEngine.Random.Range(1, 8)].Play();
        yield return new WaitForSeconds(_audioSources[0].clip.length);
        yield return NextBallComing();
    }
    
    /// <summary>
    /// Audio for a missed ball and starts a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallMissed()
    {
        int rand = UnityEngine.Random.Range(9, 13);
        _audioSources[rand].Play();
        yield return new WaitForSeconds(_audioSources[rand].clip.length);
        yield return NextBallComing();
    }

    /// <summary>
    /// Audio for next ball and calls spawnBall to start a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallComing()
    {
        if((UnityEngine.Random.Range(0,3) == 0 && _currBallNumber != -1)
            || _currBallNumber == 29) //Randomly 1/3 of the time say how many points
        {
            ExperimentLog.Log("Read the Score");
            StartCoroutine(numberSpeech.PlayExpPointsAudio(gamePoints));
            yield return new WaitForSeconds(3); //Wait 3 seconds for points audio to finish
        }
        else
        {
            AudioSource aud = NumberSpeech.PlayAudio("nextball");
            yield return new WaitForSeconds(aud.clip.length + 0.2f);
        }
        SpawnBall();
        timerStarted = true;
        oldTime = Time.time;
        newBallOk = true;
    }

    /// <summary>
    /// Untilty to shuffle the array of Exp ball locations
    /// </summary>
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

    /// <summary>
    /// Resets all the parameters of an experiment between Naive and our mode.
    /// </summary>
    private void ResetExp()
    {
        _expList.Reset();
        _currBallNumber = -1;
        gamePoints = 0;
        GameUtils.ballSpeedPointsEnabled = false;
        BallScript.GameInit = false;
        playerReady = false;
        batSound = batObj.GetComponents<AudioSource>()[0];
        batSound.mute = true;
        StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
        expResults.Clear();
    }

    private IEnumerator ReadGamePoints()
    {
        numberSpeech.PlayExpPointsAudio(gamePoints);
        yield return new WaitForSeconds(1.5f); //Wait arbiturary time till audio ends
    }

}

/// <summary>
/// Class that represents the saved data from each participant.
/// </summary>
public class ExpData
{
    public string ParticipantId { get; internal set; }
    public int BallNumber { get; internal set; }
    public int BallType { get; internal set; }
    public int BallSpeed { get; internal set; }
    public int BallResult { get; internal set; }
    public string EventTime { get; internal set; }
    public string TimerTime { get; internal set; }
    public int GamePoints { get; internal set; }

    public override string ToString()
    {
        return EventTime + "," + TimerTime + "," + ParticipantId + "," + BallNumber + "," + BallType + "," + 
            BallSpeed + "," + BallResult + "," + GamePoints;
    }
}

/// <summary>
/// Class that defines the path a ball will go in the Experiment
/// </summary>
public class BallPath
{
    public Vector3 Origin { get; set; }
    public Vector3 Destination { get; set; }
}
