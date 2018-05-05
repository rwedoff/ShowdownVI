using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
    public GameObject ourBall;
    //public GameObject naiveBall;
    public InputField nameField;
    public Text ballAndPosText;
    public Button saveExpButton;
    public Dropdown ballSpeedDropdown;
    public GameObject batObj;
    public Button startExpButton;
    public Text clockText;
    public GameObject globalSpeechGameObject;
    public GameObject menuGameObject;
    public bool IsTactileDouse;
    public bool IsCorrectionHints;
    public bool IsMidPointAnnounce;

    public static float TableEdge { get; private set; }
    public static float CenterX { get; private set; }
    //public static bool NaiveMode { private get; set; }
    public static List<ExperimentLogFile> LogFileList = new List<ExperimentLogFile>();
    public static string globalClockString;
    public static string ParticipantId { private get; set; }
    public static Snapshot CollisionSnapshot { get; set; }
    public class Snapshot { public Vector3 ballPos { get; set; } public Vector3 batPos { get; set; } }

    public enum ExpState { menus, ballInPlay, noBall }
    public static ExpState expState;

    private string clockString;
    private BallPath _currBallPath;
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
    private Timer clockTimer;
    private Timer globalTimer;
    private bool canPressStartButton;
    private int gamePoints;
    private enum HitRes { miss = 0, tipped=1, hitNotPastHalf = 2, pastHalfHit = 3, goal = 4  }
    private HitRes thisHitres;
    private NumberSpeech numberSpeech;
    private const int rightStartXPos = 37;
    private const int leftStartXPos = -37;
    private const int  centerStartXPos = 0;
    private bool IsAnnounceBall;
    private Snapshot endSnapshot;
    private Snapshot middleSnapshot;

    /// <summary>
    /// AudioSources
    /// 0: Clapping, 1-8: Good 9-13: Missed
    /// </summary>
    private AudioSource[] _audioSources;
    private bool newBallOk;
    private DateTime startTime;
    private bool canPressButton;
    private AudioSource startLeftAudio;
    private AudioSource startCenterAudio;
    private AudioSource startRightAudio;
    private AudioSource endFarLeftAudio;
    private AudioSource endCenterLeftAudio;
    private AudioSource endCenterAudio;
    private AudioSource endCenterRightAudio;
    private AudioSource endFarRightAudio;
    private AudioSource andIsAudio;
    private AudioSource tippedAudio;
    private AudioSource backwardAudio;
    private AudioSource tooForward;
    private AudioSource tooBack;
    private AudioSource moveLeftAudio;
    private AudioSource reachRight;
    private AudioSource tooRight;
    private AudioSource tooLeft;
    private AudioSource reachLeft;
    private AudioSource middleAudio;
    private AudioSource levelUpAudio;
    private int playerLevel;
    private int[] prevHits;
    private enum HintLength { full, shortLen, nonspatial }
    private HintLength oldHintLen;
    private AudioSource moveRightAudio;
    private bool firstPass;

    private void Start()
    {
        if (JoyconManager.Instance == null)
        {
            SceneManager.LoadSceneAsync("GlobalInit", LoadSceneMode.Single);
            return;
        }
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
        GameUtils.ballSpeedPointsEnabled = false;
        _currBallNumber = -1;
        CreateBallPosition();
        ShuffleArray();
        saveExpButton.onClick.AddListener(FinishExp);
        startExpButton.onClick.AddListener(StartExp);
        numberSpeech = globalSpeechGameObject.GetComponent<NumberSpeech>();
        _audioSources = GetComponents<AudioSource>();
        levelUpAudio = _audioSources[14];
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
        expState = ExpState.menus;
        playerLevel = 0;
        prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
        SetupChildAudio();
    }

    private void SetupChildAudio()
    {
        startLeftAudio = transform.Find("Start1").GetComponent<AudioSource>();
        startCenterAudio = transform.Find("Start2").GetComponent<AudioSource>();
        startRightAudio = transform.Find("Start3").GetComponent<AudioSource>();
        var farLeftAudioSources = transform.Find("End1").GetComponents<AudioSource>();
        endFarLeftAudio = farLeftAudioSources[0];
        reachLeft = farLeftAudioSources[1];
        tooLeft = farLeftAudioSources[2];
        endCenterLeftAudio = transform.Find("End2").GetComponent<AudioSource>();
        var centerAudioSources = transform.Find("End3").GetComponents<AudioSource>();
        endCenterAudio = centerAudioSources[0];
        middleAudio = centerAudioSources[1];
        tippedAudio = centerAudioSources[2];
        backwardAudio = centerAudioSources[3];
        endCenterRightAudio = transform.Find("End4").GetComponent<AudioSource>();
        var farRightAudioSources = transform.Find("End5").GetComponents<AudioSource>();
        endFarRightAudio = farRightAudioSources[0];
        reachRight = farRightAudioSources[1];
        tooRight = farRightAudioSources[2];
        var middleAudioSources = transform.Find("Guide Line").GetComponents<AudioSource>();
        andIsAudio = middleAudioSources[0];
        tooForward = middleAudioSources[1];
        tooBack = middleAudioSources[2];
        moveLeftAudio = middleAudioSources[9];
        moveRightAudio = middleAudioSources[10];
    }

    private void Update()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;

        clockText.text = clockString;

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
            expState = ExpState.noBall;
        }

        CheckHitResult();

        SetGameHints();

    }

    private void SetGameHints()
    {
        if (playerLevel <= 3)
        {
            if (IsTactileDouse)
            {
                TactileDouse();
            }
            if (IsMidPointAnnounce)
            {
                PlayMidPointAudio();
            }
            IsAnnounceBall = true;
        }

        if (IsCorrectionHints)
        {
            SaveSnapshotOfGame();
        }
    }

    private void CheckHitResult()
    {
        //Perfect hit, start new ball
        if ((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce) && maxDistance > 10)
        {
            timerStarted = false;
            StartCoroutine(HitPastHalfStartNextBall());
            return;
        }

        //Ball Falls in goal, start new ball
        if (GoalScript.ExpBallLose)
        {
            GoalScript.ExpBallLose = false;
            if (BallScript.BallHitOnce || NaiveBallScript.BallHitOnce)
            {
                StartNextBall(HitRes.tipped);
            }
            else
            {
                StartNextBall(HitRes.miss);
            }
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
                if (_currentBall != null)
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

            int timerInterval = IsAnnounceBall ? 10 : 8;

            if (Time.time > oldTime + timerInterval)
            {
                oldTime = Time.time;
                expState = ExpState.noBall;
                StartNextBall(DetermineHitRes(_currentBall));
            }
        }
    }

    private void SaveSnapshotOfGame()
    {
        if(_currentBall != null)
        {
            if(_currentBall.transform.position.z < -75 && _currentBall.transform.position.z > - 85)
            {
                endSnapshot = new Snapshot()
                {
                    ballPos = _currentBall.transform.position,
                    batPos = batObj.transform.position
                };
            }
        }
    }

    private void PlayMidPointAudio()
    {
        if (_currentBall != null)
        {
            if (firstPass && _currentBall.transform.position.z < 5 && _currentBall.transform.position.z > -5)
            {
                firstPass = false;
                var snapShotBatPos = batObj.transform.position;
                float absDist = Math.Abs(snapShotBatPos.x - GetActualXDestination());

                if (absDist > 20)
                {
                    if(snapShotBatPos.x < GetActualXDestination())
                    {
                        moveRightAudio.Play();
                    }
                    else
                    {
                        moveLeftAudio.Play();
                    }
                }
            }
        }
    }

    private void TactileDouse()
    {
        if (!BallScript.BallHitOnce)
        {
            Vector3 batPos = batObj.transform.position;
            if (_currentBall != null)
            {
                float absDist = Math.Abs(batPos.x - GetActualXDestination());
                //float distAwayFromDest = 100 - absDist;
                if (absDist < 30 && absDist > 20)
                {
                    JoyconController.RumbleJoycon(160, 320, 0.1f, 200);
                }
                else if (absDist <= 20 && absDist > 10)
                {
                    JoyconController.RumbleJoycon(160, 320, 0.3f, 200);
                }
                else if (absDist < 10)
                {
                    JoyconController.RumbleJoycon(160, 320, 0.5f, 200);
                }
            }
        }
    }
    
    private float GetActualXDestination()
    {
        var bt = _currBallPath.BallOriginType;
        int startXPos = 0;
        if (bt == BallOriginType.center)
        {
            startXPos = centerStartXPos;
        }
        else if (bt == BallOriginType.left)
        {
            startXPos = leftStartXPos;
        }
        else if (bt == BallOriginType.right)
        {
            startXPos = rightStartXPos;
        }
        return startXPos + (2 * _currBallPath.Destination.x);
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
        BallScript.BallHitOnce = false;
        NaiveBallScript.BallHitOnce = false;
        StartNextBall(HitRes.pastHalfHit);
    }

    /// <summary>
    /// Determines if the ball was hit, and didn't go past the halfway or it was missed
    /// </summary>
    /// <param name="ball"></param>
    /// <returns>HitRes.miss or HitRes.hit</returns>
    private HitRes DetermineHitRes(GameObject ball)
    {
        if((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce) && maxDistance > -50)
        {
            return HitRes.hitNotPastHalf;
        }
        else if ((BallScript.BallHitOnce || NaiveBallScript.BallHitOnce))
        {
            return HitRes.tipped;
        }
        return HitRes.miss;
    }

    /// <summary>
    /// Spawns a new ball based on the 30 balls of the experiment list.
    /// </summary>
    private IEnumerator SpawnBall()
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
                yield break;
            }

            _currBallPath = ballPositions[_currBallType];

            if (IsAnnounceBall)
            {
                if (playerLevel == 0)
                {
                    yield return AnnounceBallPos(HintLength.full);
                }
                if (playerLevel == 1)
                {
                    yield return AnnounceBallPos(HintLength.shortLen);
                }
                if (playerLevel == 2)
                {
                    yield return AnnounceBallPos(HintLength.nonspatial);
                }
            }


            if (!canPressStartButton && playerReady) //Check again to not send ball in transition.
            {
                firstPass = true;
                _currentBall = Instantiate(ourBall, _currBallPath.Origin, new Quaternion());
                expState = ExpState.ballInPlay;
                Rigidbody rb = _currentBall.GetComponent<Rigidbody>();

                _currBallSpeed = DetermineCurrBallSpeed();
                //Play Click sound
                _currentBall.GetComponents<AudioSource>()[1].Play();

                rb.AddForce(ballPositions[_currBallType].Destination * _currBallSpeed, ForceMode.Acceleration);
            }
        }
    }

    private IEnumerator AnnounceBallPos(HintLength hintLength)
    {
        if(hintLength == HintLength.full)
        {
            SetupFullHintAudio();
        }
        else if(hintLength == HintLength.shortLen)
        {
            SetupShortLenHintAudio();
        }
        else if(hintLength == HintLength.nonspatial)
        {
            SetupNonSpatialHintAudio();
        }
        //Play where the ball is starting
        if (_currBallPath.BallOriginType == BallOriginType.left) //Left Start
        {
            startLeftAudio.Play();
            yield return new WaitForSeconds(startLeftAudio.clip.length);
        }
        else if (_currBallPath.BallOriginType == BallOriginType.center) //Center Start
        {
            startCenterAudio.Play();
            yield return new WaitForSeconds(startCenterAudio.clip.length);
        }
        else if (_currBallPath.BallOriginType == BallOriginType.right) //Right Start
        {
            startRightAudio.Play();
            yield return new WaitForSeconds(startRightAudio.clip.length);
        }

        //Play And Is Going to
        if (!canPressStartButton && playerReady)
        { //Check again to not send ball in transition.
            andIsAudio.Play();
            yield return new WaitForSeconds(andIsAudio.clip.length);
        }
        else { yield break; }

        //Play the destination
        if (!canPressStartButton && playerReady)
        { //Check again to not send ball in transition.
            if (_currBallPath.BallDestType == BallDestType.farLeft)
            {
                endFarLeftAudio.Play();
                yield return new WaitForSeconds(endFarLeftAudio.clip.length);
            }
            else if (_currBallPath.BallDestType == BallDestType.centerLeft)
            {
                endCenterLeftAudio.Play();
                yield return new WaitForSeconds(endCenterLeftAudio.clip.length);
            }
            else if (_currBallPath.BallDestType == BallDestType.center)
            {
                endCenterAudio.Play();
                yield return new WaitForSeconds(endCenterAudio.clip.length);
            }
            else if (_currBallPath.BallDestType == BallDestType.centerRight)
            {
                endCenterRightAudio.Play();
                yield return new WaitForSeconds(endCenterRightAudio.clip.length);
            }
            else if (_currBallPath.BallDestType == BallDestType.farRight)
            {
                endFarRightAudio.Play();
                yield return new WaitForSeconds(endFarRightAudio.clip.length);
            }
        }
        else { yield break; }
    }

    private void SetupFullHintAudio()
    {
        if(oldHintLen != HintLength.full)
        {
            startLeftAudio = transform.Find("Start1").GetComponent<AudioSource>();
            startCenterAudio = transform.Find("Start2").GetComponent<AudioSource>();
            startRightAudio = transform.Find("Start3").GetComponent<AudioSource>();
            var farLeftAudioSources = transform.Find("End1").GetComponents<AudioSource>();
            endFarLeftAudio = farLeftAudioSources[0];
            endCenterLeftAudio = transform.Find("End2").GetComponent<AudioSource>();
            var centerAudioSources = transform.Find("End3").GetComponents<AudioSource>();
            endCenterAudio = centerAudioSources[0];
            endCenterRightAudio = transform.Find("End4").GetComponent<AudioSource>();
            var farRightAudioSources = transform.Find("End5").GetComponents<AudioSource>();
            endFarRightAudio = farRightAudioSources[0];
            var middleAudioSources = transform.Find("Guide Line").GetComponents<AudioSource>();
            andIsAudio = middleAudioSources[0];
        }
        oldHintLen = HintLength.full;
    }

    private void SetupShortLenHintAudio()
    {
        if (oldHintLen != HintLength.shortLen)
        {
            startLeftAudio = transform.Find("Start1").GetComponents<AudioSource>()[1];
            startCenterAudio = transform.Find("Start2").GetComponents<AudioSource>()[1];
            startRightAudio = transform.Find("Start3").GetComponents<AudioSource>()[1];
            var farLeftAudioSources = transform.Find("End1").GetComponents<AudioSource>();
            endFarLeftAudio = farLeftAudioSources[3];
            endCenterLeftAudio = transform.Find("End2").GetComponent<AudioSource>();
            var centerAudioSources = transform.Find("End3").GetComponents<AudioSource>();
            endCenterAudio = centerAudioSources[0];
            endCenterRightAudio = transform.Find("End4").GetComponent<AudioSource>();
            var farRightAudioSources = transform.Find("End5").GetComponents<AudioSource>();
            endFarRightAudio = farRightAudioSources[3];
            var middleAudioSources = transform.Find("Guide Line").GetComponents<AudioSource>();
            andIsAudio = middleAudioSources[3];
        }
        oldHintLen = HintLength.shortLen;
    }

    private void SetupNonSpatialHintAudio()
    {
        if (oldHintLen != HintLength.nonspatial)
        {
            var middleAudioSources = transform.Find("Guide Line").GetComponents<AudioSource>();
            startLeftAudio = middleAudioSources[4];
            startCenterAudio = middleAudioSources[6];
            startRightAudio = middleAudioSources[8];
            endFarLeftAudio = middleAudioSources[4];
            endCenterLeftAudio = middleAudioSources[5];
            endCenterAudio = middleAudioSources[6];
            endCenterRightAudio = middleAudioSources[7];
            endFarRightAudio = middleAudioSources[8];
            andIsAudio = middleAudioSources[3];
        }
        oldHintLen = HintLength.nonspatial;
    }

    private int DetermineCurrBallSpeed()
    {
        if (ballSpeedDropdown.value == 0) //Dynamic
        {
            if(playerLevel <= 4)
            {
                return 40;
            }
            else if (playerLevel == 5)
            {
                return 50;
            }
            else if(playerLevel == 6)
            {
                return 60;
            }
            else if(playerLevel >= 7)
            {
                return 70;
            }
        }
        else if (ballSpeedDropdown.value == 1) //Slow
        {
            return 40; //Was 75 or 40
        }
        else if (ballSpeedDropdown.value == 2) //Medium
        {
            return 75;
        }
        else if (ballSpeedDropdown.value == 3) //Fast
        {
            return 125;
        }
        else if (ballSpeedDropdown.value == 4) //Random
        {
            return UnityEngine.Random.Range(75, 250);
        }
        return 40;
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
                    StartCoroutine((hitres != HitRes.miss && hitres != HitRes.tipped) ? NextBallHit() : NextBallMissed(hitres));
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
                menuGameObject.SetActive(true);
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
                menuGameObject.SetActive(true);
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
            "Ball_Result [Miss->0|1 : MaybeHit->2 : HitPastHalf->3 : Goal->4]" + "," + "Game_Points");
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
        //string naiveModeStr = NaiveMode ? "_navie_" : "_our_";
        
        var path = EditorUtility.SaveFilePanel(
              "Save Experiment as CSV",
              "",
              nameField.text + "_exp.csv",
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
        prevHits[_currBallNumber % 6] = 1;
        if (CheckLevelUp())
        {
            playerLevel ++;
            levelUpAudio.Play();
            yield return new WaitForSeconds(levelUpAudio.clip.length);
        }
        _audioSources[0].Play();
        _audioSources[UnityEngine.Random.Range(1, 8)].Play();
        yield return new WaitForSeconds(_audioSources[0].clip.length);
        yield return NextBallComing();
    }

    /// <summary>
    /// Checks if a player can level by checking if the last 6 hits were hit or not.
    /// </summary>
    /// <returns></returns>
    private bool CheckLevelUp()
    {
        int hits = 0;
        foreach(int h in prevHits)
        {
            if(h == 1)
            {
                hits++;
            }
        }
        if(hits > 3) // 4 out of 6 hits, level up!
        {
            prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
            Debug.Log("Level Up: " + (playerLevel + 1));
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Audio for a missed ball and starts a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallMissed(HitRes hitRes)
    {
        prevHits[_currBallNumber % 6] = 0;
        //Randomly, 1/3 of the time, play a random lose voice sound effect
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            int rand = UnityEngine.Random.Range(9, 13);
            _audioSources[rand].Play();
            yield return new WaitForSeconds(_audioSources[rand].clip.length);
        }

        if (IsCorrectionHints)
        {
            yield return ReadHitCorrection(hitRes);
        }

        yield return NextBallComing();
    }

    private IEnumerator ReadHitCorrection(HitRes hitRes)
    {
        var snapShotBatPos = endSnapshot.batPos;
        var snapShotBallPos = endSnapshot.ballPos;
        float absDist = Math.Abs(snapShotBatPos.x - snapShotBallPos.x);
        if (hitRes == HitRes.tipped)
        {
            if (CollisionSnapshot.ballPos.x < CollisionSnapshot.batPos.x - 5)
            {
                tippedAudio.Play();
                yield return new WaitForSeconds(tippedAudio.clip.length);
                ExperimentLog.Log("Tipped to the left");
                reachLeft.Play();
                yield return new WaitForSeconds(reachLeft.clip.length);
            }
            else if(CollisionSnapshot.ballPos.x > CollisionSnapshot.batPos.x + 5)
            {
                tippedAudio.Play();
                yield return new WaitForSeconds(tippedAudio.clip.length);
                reachRight.Play();
                yield return new WaitForSeconds(reachRight.clip.length);
                ExperimentLog.Log("Tipped to the right");
            }
            else if (CollisionSnapshot.ballPos.z < CollisionSnapshot.batPos.z)
            {
                ExperimentLog.Log("You hit the ball backward");
                backwardAudio.Play();
                yield return new WaitForSeconds(backwardAudio.clip.length);
            }
        }
        else if (absDist < 10)
        {
            if (snapShotBatPos.z > snapShotBallPos.z)
            {
                //Reached too far forward too soon.
                tooForward.Play();
                yield return new WaitForSeconds(tooForward.clip.length);
            }
            else
            {
                //Reached too far back
                tooBack.Play();
                yield return new WaitForSeconds(tooBack.clip.length);
            }
        }
        else if (snapShotBallPos.x > 0 && snapShotBallPos.x > snapShotBatPos.x)
        {
            //Reach further to the right
            float distOff = snapShotBallPos.x - snapShotBatPos.x;
            reachRight.Play();
            yield return new WaitForSeconds(reachRight.clip.length);
            StartCoroutine(numberSpeech.PlayFancyNumberAudio((int)distOff));
            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x > 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Too far to the right
            float distOff = snapShotBatPos.x - snapShotBallPos.x;
            tooRight.Play();
            yield return new WaitForSeconds(tooRight.clip.length);
            StartCoroutine(numberSpeech.PlayFancyNumberAudio((int)distOff));
            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x > snapShotBatPos.x)
        {
            //Too far to the left
            float distOff = Math.Abs(snapShotBatPos.x - snapShotBallPos.x);
            tooLeft.Play();
            yield return new WaitForSeconds(tooLeft.clip.length);
            StartCoroutine(numberSpeech.PlayFancyNumberAudio((int)distOff));
            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x < 0 && snapShotBallPos.x < snapShotBatPos.x)
        {
            //Reach futher to the left
            float distOff = Math.Abs(snapShotBallPos.x - snapShotBatPos.x);
            reachLeft.Play();
            yield return new WaitForSeconds(reachLeft.clip.length);
            StartCoroutine(numberSpeech.PlayFancyNumberAudio((int)distOff));
            yield return new WaitForSeconds(2.5f);
        }
        else if (snapShotBallPos.x == 0)
        {
            //Put it right in the middle
            middleAudio.Play();
            yield return new WaitForSeconds(middleAudio.clip.length);
        }
        yield break;
    }

    /// <summary>
    /// Audio for next ball and calls spawnBall to start a new ball
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextBallComing()
    {
        if ((UnityEngine.Random.Range(0, 3) == 0 && _currBallNumber != -1 && gamePoints != 1)
                || _currBallNumber == 29) //Randomly 1/3 of the time say how many points
        {
            ExperimentLog.Log("Read the Score");
            StartCoroutine(numberSpeech.PlayExpPointsAudio(gamePoints));
            yield return new WaitForSeconds(4.5f); //Wait 4.5 seconds for points audio to finish
        }
        else if (!IsAnnounceBall)
        {
            AudioSource aud = NumberSpeech.PlayAudio("nextball");
            yield return new WaitForSeconds(aud.clip.length + 0.2f);
        }

        yield return SpawnBall();
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
    /// Creation of list of ball positions and their destinations.
    /// </summary>
    private void CreateBallPosition()
    {
        //Left Start
        ballPositions.Add(0, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(1, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(2, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(3, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(31, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(4, new BallPath
        {
            Origin = new Vector3(leftStartXPos, 5, 110),
            Destination = new Vector3(42, 5, -110),
            BallOriginType = BallOriginType.left,
            BallDestType = BallDestType.farRight
        });

        //Center Start
        ballPositions.Add(5, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(6, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(7, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(8, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(11, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(9, new BallPath
        {
            Origin = new Vector3(centerStartXPos, 5, 110),
            Destination = new Vector3(21, 5, -110),
            BallOriginType = BallOriginType.center,
            BallDestType = BallDestType.farRight
        });

        //Right Start
        ballPositions.Add(10, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-42, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farLeft
        });
        ballPositions.Add(11, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-31, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerLeft
        });
        ballPositions.Add(12, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-21, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.center
        });
        ballPositions.Add(13, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(-11, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.centerRight
        });
        ballPositions.Add(14, new BallPath
        {
            Origin = new Vector3(rightStartXPos, 5, 110),
            Destination = new Vector3(0, 5, -110),
            BallOriginType = BallOriginType.right,
            BallDestType = BallDestType.farRight
        });
    }

    /// <summary>
    /// Resets all the parameters of an experiment between Naive and our mode.
    /// </summary>
    private void ResetExp()
    {
        _expList.Reset();
        _currBallNumber = -1;
        gamePoints = 0;
        playerLevel = 0;
        GameUtils.ballSpeedPointsEnabled = false;
        BallScript.GameInit = false;
        playerReady = false;
        batSound = batObj.GetComponents<AudioSource>()[0];
        batSound.mute = true;
        StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
        prevHits = new int[6] { 0, 0, 0, 0, 0, 0 };
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
    public BallOriginType BallOriginType { get; set; }
    public BallDestType BallDestType { get; set; }
}
public enum BallOriginType { left, center, right }
public enum BallDestType { farLeft, centerLeft, center, centerRight, farRight }
