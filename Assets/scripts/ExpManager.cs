using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExpManager : MonoBehaviour
{
    public GameObject ballObject;
    public InputField nameField;
    public Text ballAndPosText;
    public Button nextBallButton;
    public Button saveExpButton;
    public Dropdown hitResultDropdown;
    public Button MissedButton;
    public Dropdown ballSpeedDropdown;

    private GameObject _currentBall;
    private Dictionary<int, BallPath> ballPositions= new Dictionary<int, BallPath>();
    private IEnumerator<int> _expList;
    private int _currBallNumber;
    private string _participantName;
    private List<ExpData> expResults = new List<ExpData>();
    private int _currBallType;
    private int _currBallSpeed;
    private float _oldTime;
    private bool _timerStarted;
    private bool _newBallOk;

    /// <summary>
    /// AudioSources
    /// 0: Clapping, 1-5: Good 6-8: Missed
    /// </summary>
    private AudioSource[] _audioSources;

    private void ShuffleArray()
    {
        List<int> expListLoc = new List<int>();
        for (int i = 0; i <= 14; i++) //Range of positions
        {
            for (int j = 0; j <= 3; j++) //Times per position
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

    [UsedImplicitly]
    private void Start()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
        GameUtils.ballSpeedPointsEnabled = false;
        _timerStarted = true;
        _newBallOk = true;
        _currBallNumber = -1;
        CreateBallPosition();
        ShuffleArray();
        nextBallButton.onClick.AddListener(() => StartNextBall(true));
        saveExpButton.onClick.AddListener(SaveCsv);
        MissedButton.onClick.AddListener(() => StartNextBall(false));
        _audioSources = GetComponents<AudioSource>();
    }

    private void SpawnBall()
    {
        _currBallType = _expList.Current;
        _currBallNumber++;
        ballAndPosText.text = "Ball: " + _currBallNumber + "   Position: " + _currBallType;
        bool isNewBallAvail = _expList.MoveNext();
        if (!isNewBallAvail)
        {
            return;
        }
        _currentBall = Instantiate(ballObject, ballPositions[_currBallType].Origin, new Quaternion());
        Rigidbody rb = _currentBall.GetComponent<Rigidbody>();
        _currentBall.GetComponents<AudioSource>()[1].Play();
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

    [UsedImplicitly]
    private void Update()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;

        if (_timerStarted)
        {
            if (_currBallNumber == -1 || Time.time > _oldTime + 3)
            {
                _timerStarted = false;
                _newBallOk = true;
            }
        }
    }

    private void StartNextBall(bool hit)
    {
        if (_newBallOk)
        {
            _newBallOk = false;
            Debug.Log("Sending Ball");
            _timerStarted = true;
            _oldTime = Time.time;
            if (_currBallNumber != -1)
                CollectExpData();
            Destroy(_currentBall);
            if (_currBallNumber != -1)
            {
                StartCoroutine(hit ? NextBallHit() : NextBallMissed());
            }
            else
            {
                StartCoroutine(NextBallComing());
            }
        }

    }
    
    private void CollectExpData()
    {
        expResults.Add(new ExpData()
        {
            ParticipantId = nameField.text,
            BallNumber = _currBallNumber,
            BallType = _currBallType,
            BallSpeed = _currBallSpeed,
            BallResult = hitResultDropdown.value
        });
        hitResultDropdown.value = 0;
    }

    private void SaveCsv()
    {
        CollectExpData();
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
                SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
                break;

            // Cancel.
            case 1:
                break;

            // Quit Without saving.
            case 2:
                SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
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
        AudioSource aud = NumberSpeech.PlayAudio(19);
        yield return new WaitForSeconds(aud.clip.length + 0.2f);
        SpawnBall();
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
