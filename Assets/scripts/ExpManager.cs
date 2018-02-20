using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
    public GameObject ballObject;
    public InputField nameField;
    public Text ballAndPosText;
    public Button saveExpButton;
    public Dropdown ballSpeedDropdown;
    public GameObject batObj;

    private GameObject _currentBall;
    private Dictionary<int, BallPath> ballPositions= new Dictionary<int, BallPath>();
    private IEnumerator<int> _expList;
    private int _currBallNumber;
    private string _participantName= "";
    private List<ExpData> expResults = new List<ExpData>();
    private int _currBallType;
    private int _currBallSpeed;
    private bool playerReady;
    private AudioSource batSound;

    private float oldTime;
    private bool timerStarted;
    private bool canSendBall;
    private float maxDistance;

    /// <summary>
    /// AudioSources
    /// 0: Clapping, 1-5: Good 6-8: Missed
    /// </summary>
    private AudioSource[] _audioSources;
    private bool newBallOk;

    private void Start()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
        GameUtils.ballSpeedPointsEnabled = false;
        _currBallNumber = -1;
        CreateBallPosition();
        ShuffleArray();
        saveExpButton.onClick.AddListener(FinishExp);
        _audioSources = GetComponents<AudioSource>();
        BallScript.GameInit = false;
        playerReady = false;
        batSound = batObj.GetComponents<AudioSource>()[0];
        batSound.mute = true;
        StartCoroutine(GameUtils.PlayIntroMusic());
        newBallOk = true;
    }

    private void Update()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;

        //Check ball state
        //DEBUG ONLY
        //if(true)
        //END DEBUG
        //Wait for player initalization
        if (!playerReady)
        {
            batSound.mute = true;
            if (JoyconController.Shoulder2Pressed)
            {
                playerReady = true;
                Debug.Log("Player Ready: Starting Exp");
                StartNextBall(true);
            }
        }
        else
        {
            Time.timeScale = 1;
            BallScript.GameInit = false;
            batSound.mute = false;
        }

        //Perfect hit, start new ball
        if(BallScript.BallHitOnce && maxDistance > 10)
        {
            timerStarted = false;
            BallScript.BallHitOnce = false;
            StartNextBall(true);
            return;
        }

        //Ball Falls in goal, start new ball
        if (GoalScript.ExpBallLose)
        {
            GoalScript.ExpBallLose = false;
            StartNextBall(false);
        }

        //Wait for result of hit
        if (timerStarted)
        {
            if (BallScript.BallHitOnce)
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

    private bool DetermineHit(GameObject ball)
    {
        if(BallScript.BallHitOnce && maxDistance > -50)
        {
            return true;
        }
        return false;
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
            _currBallSpeed = Random.Range(75, 250);
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

    private void StartNextBall(bool hit)
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
                    CollectExpData(hit);
                Destroy(_currentBall);
                if (_currBallNumber != -1)
                {
                    StartCoroutine(hit ? NextBallHit() : NextBallMissed());
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
    
    private void CollectExpData(bool hit)
    {
        expResults.Add(new ExpData()
        {
            ParticipantId = nameField.text,
            BallNumber = _currBallNumber,
            BallType = _currBallType,
            BallSpeed = _currBallSpeed,
            BallResult = hit ? 0 : 1
        });
    }

    private void FinishExp()
    {
        StartCoroutine(SaveCSV());
    }

    private IEnumerator SaveCSV()
    {
        NumberSpeech.PlayAudio("thanks");
        yield return new WaitForSeconds(2);
        int option = EditorUtility.DisplayDialogComplex("Finish Experiment",
            "Are you sure you want to finish this experiment?",
            "Save",
            "Cancel",
            "Quit Without Saving");
        switch (option)
        {
            // Save
            case 0:
                CreateFile();
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
                break;

            // Cancel.
            case 1:
                break;

            // Quit Without saving.
            case 2:
                SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
                break;

            default:
                Debug.LogError("Unrecognized option.");
                break;
        }
    }

    private void CreateFile()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Participant_Id" + "," + "Ball_Number" + "," + "Ball_Type" + ", " + "Ball_Speed" + "," + "Ball_Result");
        foreach (var data in expResults)
        {
            sb.AppendLine(data.ToString());
        }
        var path = EditorUtility.SaveFilePanel(
              "Save Experiment as CSV",
              "",
              _participantName + "exp.csv",
              "csv");

        if (path.Length != 0)
        {
            File.WriteAllBytes(path, new UTF8Encoding().GetBytes(sb.ToString()));
        }

    }

    private IEnumerator NextBallHit()
    {
        _audioSources[0].Play();
        _audioSources[Random.Range(1, 5)].Play();
        yield return new WaitForSeconds(_audioSources[0].clip.length);
        yield return NextBallComing();
    }

    private IEnumerator NextBallMissed()
    {
        int rand = Random.Range(6, 8);
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
