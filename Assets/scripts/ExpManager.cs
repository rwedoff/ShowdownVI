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
    public Button nextBallButton;
    public Button saveExpButton;
    public Dropdown hitResultDropdown;

    private GameObject currentBall;
    private Dictionary<int, BallPath> ballPositions= new Dictionary<int, BallPath>();
    private IEnumerator<int> expList;
    private int currBallNumber;
    private string participantName;
    private List<ExpData> expResults = new List<ExpData>();
    private int currBallType;
    private int currBallSpeed;

    private void Shuffle()
    {
        List<int> _expList = new List<int>();
        for (int i = 0; i <= 14; i++) //Range of positions
        {
            for (int j = 0; j <= 3; j++) //Times per position
            {
                _expList.Add(i);
            }
        }
        System.Random rng = new System.Random();
        int n = _expList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = _expList[k];
            _expList[k] = _expList[n];
            _expList[n] = value;
        }
        expList = _expList.GetEnumerator();
    }

    private void Start()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
        GameUtils.ballSpeedPointsEnabled = false;
        currBallNumber = -1;
        CreateBallPosition();
        Shuffle();
        nextBallButton.onClick.AddListener(StartNextBall);
        saveExpButton.onClick.AddListener(SaveCSV);
    }

    private bool SpawnBall()
    {
        currBallType = expList.Current;
        currBallNumber++;
        ballAndPosText.text = "Ball: " + currBallNumber + "   Position: " + currBallType;
        bool isNewBallAvail = expList.MoveNext();
        if (!isNewBallAvail)
        {
            return isNewBallAvail;
        }
        currentBall = Instantiate(ballObject, ballPositions[currBallType].origin, new Quaternion());
        Rigidbody rb = currentBall.GetComponent<Rigidbody>();
        currentBall.GetComponents<AudioSource>()[1].Play();
        //currBallSpeed = Random.Range(75, 250); //Used for random ball speeds
        currBallSpeed = 125;
        rb.AddForce(ballPositions[currBallType].destination * currBallSpeed, ForceMode.Acceleration);
        return isNewBallAvail;
    }

    private void CreateBallPosition()
    {
        //Left Start
        ballPositions.Add(0, new BallPath
        {
            origin = new Vector3(-42, 3, 110),
            destination = new Vector3(0, 3, -110)
        });
        ballPositions.Add(1, new BallPath
        {
            origin = new Vector3(-42, 3, 110),
            destination = new Vector3(11, 3, -110)
        });
        ballPositions.Add(2, new BallPath
        {
            origin = new Vector3(-42, 3, 110),
            destination = new Vector3(21, 3, -110)
        });
        ballPositions.Add(3, new BallPath
        {
            origin = new Vector3(-42, 3, 110),
            destination = new Vector3(31, 3, -110)
        });
        ballPositions.Add(4, new BallPath
        {
            origin = new Vector3(-42, 3, 110),
            destination = new Vector3(42, 3, -110)
        });

        //Middle Start
        ballPositions.Add(5, new BallPath
        {
            origin = new Vector3(0, 3, 110),
            destination = new Vector3(-21, 3, -110)
        });
        ballPositions.Add(6, new BallPath
        {
            origin = new Vector3(0, 3, 110),
            destination = new Vector3(-11, 3, -110)
        });
        ballPositions.Add(7, new BallPath
        {
            origin = new Vector3(0, 3, 110),
            destination = new Vector3(0, 3, -110)
        });
        ballPositions.Add(8, new BallPath
        {
            origin = new Vector3(0, 3, 110),
            destination = new Vector3(11, 3, -110)
        });
        ballPositions.Add(9, new BallPath
        {
            origin = new Vector3(0, 3, 110),
            destination = new Vector3(21, 3, -110)
        });

        //Right Start
        ballPositions.Add(10, new BallPath
        {
            origin = new Vector3(42, 3, 110),
            destination = new Vector3(0, 3, -110)
        });
        ballPositions.Add(11, new BallPath
        {
            origin = new Vector3(42, 3, 110),
            destination = new Vector3(-11, 3, -110)
        });
        ballPositions.Add(12, new BallPath
        {
            origin = new Vector3(42, 3, 110),
            destination = new Vector3(-21, 3, -110)
        });
        ballPositions.Add(13, new BallPath
        {
            origin = new Vector3(42, 3, 110),
            destination = new Vector3(-31, 3, -110)
        });
        ballPositions.Add(14, new BallPath
        {
            origin = new Vector3(42, 3, 110),
            destination = new Vector3(-42, 3, -110)
        });
    }

    private void Update()
    {
        GameUtils.playState = GameUtils.GamePlayState.ExpMode;
    }

    private void StartNextBall()
    {
        if(currBallNumber != -1)
            CollectExpData();
        Destroy(currentBall);
        StartCoroutine(NextBallSpeech());
    }
    
    private void CollectExpData()
    {
        expResults.Add(new ExpData()
        {
            participantId = nameField.text,
            ballNumber = currBallNumber,
            ballType = currBallType,
            ballSpeed = currBallSpeed,
            ballResult = hitResultDropdown.value
        });
        hitResultDropdown.value = 0;
    }

    private void SaveCSV()
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
              participantName + "exp.csv",
              "csv");

        if (path.Length != 0)
        {
            if (sb != null)
                File.WriteAllBytes(path, new UTF8Encoding().GetBytes(sb.ToString()));
        }

    }

    private IEnumerator NextBallSpeech()
    {
        AudioSource aud = NumberSpeech.PlayAudio(19);
        yield return new WaitForSeconds(aud.clip.length + 0.5f);
        SpawnBall();
    }
}

public class ExpData
{
    public string participantId { get; set; }
    public int ballNumber { get; set; }
    public int ballType { get; set; }
    public int ballSpeed { get; set; }
    public int ballResult { get; set; }

    public override string ToString()
    {
        return participantId + "," + ballNumber + "," + ballType + "," + ballSpeed + "," + ballResult;
    }
}

public class BallPath
{
    public Vector3 origin { get; set; }
    public Vector3 destination { get; set; }
}
