using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    public Button AudioOnlyButton;
    public Button freeButton;
    public Text expMenuText;
    public InputField partInputField;
    public Button startRightHand;
    public Button startLeftHand;
    public InputField expInputField;
    public GameObject bodySourceViewObj;
    public Button calibrateButton;
    public GameObject calibrationGO;
    public GameObject mainMenuGO;
    public GameObject startMenuGO;
    public GameObject menuGameObject;
    public Button tactileAndAudioButton;
    public Button startExpButton;
    public Button finishExpButton;

    public static AudioSource clickAudioSource;
    public static AudioSource hoverTempAudioSource;

    private static List<Selectable> currSelectableList;
    private int buttonIndex;

    private static List<Selectable> mainSelectableList;
    private static List<AudioSource> mainAudioSouceList;
    private List<Selectable> calibSelectableList;
    private List<AudioSource> calibAudioSourceList;
    private List<Selectable> startSelectableList;
    private List<AudioSource> startAudioSourceList;
    public static List<Selectable> expSelectableList;
    private List<AudioSource> expAudioList;

    private int currSelectableIndex;
    private Queue<AudioSource> playingAudioQueue;
    private AudioSource playingAudio;
    private static List<AudioSource> currAudioList;

    // Use this for initialization
    void Start () {
        clickAudioSource = GetComponent<AudioSource>();
        calibrationGO.SetActive(false);
        mainMenuGO.SetActive(false);

        SetupButtonClickListeners();

        startSelectableList = new List<Selectable>() { partInputField, startLeftHand, startRightHand };
        calibSelectableList = new List<Selectable>() { calibrateButton };
        mainSelectableList = new List<Selectable>() { tactileAndAudioButton, AudioOnlyButton, freeButton };
        expSelectableList = new List<Selectable>() { startExpButton, finishExpButton };
        currSelectableList = startSelectableList;
        SetupSelectableAudioSources();
        currSelectableIndex = 0;
        ToggleTopButton();
        playingAudioQueue = new Queue<AudioSource>();
        var textInput = currSelectableList[currSelectableIndex] as InputField;
        textInput.ActivateInputField();
    }

    private void SetupSelectableAudioSources()
    {
        startAudioSourceList = new List<AudioSource>()
        {
            partInputField.GetComponent<AudioSource>(),
            startLeftHand.GetComponent<AudioSource>(),
            startRightHand.GetComponent<AudioSource>()
        };

        calibAudioSourceList = new List<AudioSource>()
        {
            calibrateButton.GetComponent<AudioSource>()
        };

        mainAudioSouceList = new List<AudioSource>()
        {
            tactileAndAudioButton.GetComponent<AudioSource>(),
            AudioOnlyButton.GetComponent<AudioSource>(),
            freeButton.GetComponent<AudioSource>()
        };

        expAudioList = new List<AudioSource>()
        {
            startExpButton.GetComponent<AudioSource>(),
            finishExpButton.GetComponent<AudioSource>()
        };

        currAudioList = startAudioSourceList;
    }

    private void SetupButtonClickListeners()
    {
        AudioOnlyButton.onClick.AddListener(() => {
            ExperimentLog.Log("Pressed Exp Mode", "Menu");
            ExpManager.TactileAndAudio = false;
            menuGameObject.SetActive(false);
            mainMenuGO.SetActive(true);
            expMenuText.text = "Audio Only Exp";
            Time.timeScale = 1;
            if (!SceneManager.GetActiveScene().name.Equals("Master"))
            {
                SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
            }
            currAudioList = expAudioList;
            currSelectableList = expSelectableList;
            ToggleTopButton();
        });

        tactileAndAudioButton.onClick.AddListener(() =>
        {
            ExperimentLog.Log("Pressed Naive Mode", "Menu");
            ExpManager.TactileAndAudio = true;
            menuGameObject.SetActive(false);
            mainMenuGO.SetActive(true);
            Time.timeScale = 1;
            expMenuText.text = "Tactile & Audio Exp";
            if (!SceneManager.GetActiveScene().name.Equals("Master"))
            {
                SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
            }
            currAudioList = expAudioList;
            currSelectableList = expSelectableList;
            ToggleTopButton();
        });

        freeButton.onClick.AddListener(() => {
            Time.timeScale = 1;
            ExperimentLog.Log("Pressed Free play Mode", "Menu");
            SceneManager.LoadSceneAsync("SinglePlayer", LoadSceneMode.Single);
        });

        startRightHand.onClick.AddListener(() =>
        {
            if (!partInputField.text.Equals(""))
            {
                ExperimentLog.Log("Clicked [Start Right Hand]", tag: "pre-menu");
                StartCalbMenu(false);
            }
            else
            {
                ExperimentLog.Log("Missing Participant ID", tag: "pre-menu");
                Debug.LogWarning("Missing Participant ID");
                GetComponents<AudioSource>()[1].Play();
            }
            currSelectableList = calibSelectableList;
            currAudioList = calibAudioSourceList;
            ToggleTopButton();
        });

        startLeftHand.onClick.AddListener(() =>
        {
            if (!partInputField.text.Equals(""))
            {
                ExperimentLog.Log("Clicked [Start Right Hand]", tag: "pre-menu");
                StartCalbMenu(true);
            }
            else
            {
                ExperimentLog.Log("Missing Participant ID", tag: "pre-menu");
                Debug.LogWarning("Missing Participant ID");
                GetComponents<AudioSource>()[1].Play();
            }
            currSelectableList = calibSelectableList;
            currAudioList = calibAudioSourceList;
            ToggleTopButton();
        });

        calibrateButton.onClick.AddListener(() =>
        {
            //If a JoyCon Button is pressed for the first time, then
            //the calibration in Exp Manager will be set.
            JoyconController.ButtonPressed = true;
            calibrationGO.SetActive(false);
            mainMenuGO.SetActive(true);
            currSelectableList = mainSelectableList;
            currAudioList = mainAudioSouceList;
            ToggleTopButton();
        });
    }

    /// <summary>
    /// Toggles the top button in the menu active
    /// </summary>
    private void ToggleTopButton()
    {
        currSelectableIndex = 0;
        var tempButton = currSelectableList[currSelectableIndex];
        var cb = tempButton.colors;
        cb.normalColor = Color.cyan;
        tempButton.colors = cb;
        AddAudioToPlayingList(currAudioList[currSelectableIndex]);
    }

    private void Update()
    {
        KeyBoardMenuControl();
        CheckAndPlayAudio();
    }

    private void KeyBoardMenuControl()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            ToggleHighlightedSelectable(true);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) || 
            Input.GetKeyUp(KeyCode.Tab)
            || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            ToggleHighlightedSelectable(false);
        }
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            ClickSelectedButton();
        }
    }

    /// <summary>
    /// Clicks the button in the menu. This is called from a keyboard button
    /// press to click the menu button.
    /// </summary>
    public void ClickSelectedButton()
    {
        if (currSelectableList == startSelectableList && currSelectableIndex == 0)
        {
            var textInput = currSelectableList[currSelectableIndex] as InputField;
            textInput.ActivateInputField();
        }
        else
        {
            CurrSelectableAsButtonClick();
        }
        clickAudioSource.Play();
        currSelectableIndex = 0;
    }

    /// <summary>
    /// Treats the current selectable as a button and calls the click method
    /// </summary>
    private void CurrSelectableAsButtonClick()
    {
        if(currSelectableList == calibSelectableList)
        {
            var b = currSelectableList[0] as Button;
            b.onClick.Invoke();
            return; 
        }
        var button = currSelectableList[currSelectableIndex] as Button;
        button.onClick.Invoke();
        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        button.colors = cb;
    }

    /// <summary>
    /// Highlights the currently selected button in the menu. 
    /// Also includes logic to cycle though the menus once the 
    /// </summary>
    /// <param name="upArrow"></param>
    public void ToggleHighlightedSelectable(bool upArrow)
    {
        if(currSelectableList == calibSelectableList)
        {
            currSelectableIndex = 0;
            AddAudioToPlayingList(currAudioList[currSelectableIndex]);
            return;
        }
        int endOfList = currSelectableList.Count;
        var tempButton = currSelectableList[currSelectableIndex];
        ColorBlock cb = tempButton.colors;
        cb.normalColor = Color.white;
        tempButton.colors = cb;

        if (!upArrow)
        {
            currSelectableIndex++;
            if (currSelectableIndex >= endOfList)
            {
                currSelectableIndex = 0;
            }
        }
        else
        {
            currSelectableIndex--;
            if (currSelectableIndex < 0)
            {
                currSelectableIndex = endOfList - 1;
            }
        }
        tempButton = currSelectableList[currSelectableIndex];
        cb = tempButton.colors;
        cb.normalColor = Color.cyan;
        tempButton.colors = cb;
        AddAudioToPlayingList(currAudioList[currSelectableIndex]);
        if (currSelectableList == startSelectableList && currSelectableIndex == 0)
        {
            var textInput = currSelectableList[currSelectableIndex] as InputField;
            textInput.ActivateInputField();
        }
    }

    /// <summary>
    /// Adds an AudioSource to a queue to play for the accessible menu
    /// </summary>
    /// <param name="newAudio"></param>
    private void AddAudioToPlayingList(AudioSource newAudio)
    {
        if(newAudio != null && playingAudioQueue != null)
            playingAudioQueue.Enqueue(newAudio);
    }

    /// <summary>
    /// Plays audio if there is audio in the AudioSource queue
    /// </summary>
    private void CheckAndPlayAudio()
    {
        if (playingAudioQueue.Count != 0)
        {
            if (playingAudio != null && playingAudio.isPlaying) {
                playingAudio.Stop();
            }
            playingAudio = playingAudioQueue.Dequeue();
            playingAudio.Play();
        }
    }

    /// <summary>
    /// Sets Lefty mode, fills in the participant ID in Exp Panel and hides the Pre-menu
    /// </summary>
    /// <param name="isLefty"></param>
    private void StartCalbMenu(bool isLefty)
    {
        expInputField.text = partInputField.text;
        startMenuGO.SetActive(false);
        mainMenuGO.SetActive(false);
        calibrationGO.SetActive(true);
        var script = bodySourceViewObj.GetComponent<BodySourceView>();
        script.SetLeftyToggle(isLefty);
    }

    /// <summary>
    /// Static helper used in ExpManager to reset the selectable list and audio list 
    /// to the main menu
    /// </summary>
    public static void SetMainSelectableAndAudio()
    {
        currSelectableList = mainSelectableList;
        currAudioList = mainAudioSouceList;
    }
}
