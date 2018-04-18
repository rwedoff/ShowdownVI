using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    public Button expButton;
    public Button freeButton;
    public Button naiveButton;
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

	// Use this for initialization
	void Start () {
        calibrationGO.SetActive(false);
        mainMenuGO.SetActive(false);

        expButton.onClick.AddListener(() => {
            ExperimentLog.Log("Pressed Exp Mode", "Menu");
            ExpManager.NaiveMode = false;
            menuGameObject.SetActive(false);
            mainMenuGO.SetActive(true); //TODO Debatable
            expMenuText.text = "Our Ball Exp";
            Time.timeScale = 1;
            if (!SceneManager.GetActiveScene().name.Equals("Master"))
            {
                SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
            }
        });

        naiveButton.onClick.AddListener(() => {
            ExperimentLog.Log("Pressed Naive Mode", "Menu");
            ExpManager.NaiveMode = true;
            menuGameObject.SetActive(false);
            mainMenuGO.SetActive(true); //TODO Debatable
            Time.timeScale = 1;
            expMenuText.text = "Naive Ball Exp";
            if (!SceneManager.GetActiveScene().name.Equals("Master"))
            {
                SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
            }
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
                ExperimentLog.Log("Missing Participant ID", tag:"pre-menu");
                Debug.LogWarning("Missing Participant ID");
            }
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
            }
        });

        calibrateButton.onClick.AddListener(() =>
        {
            //If a JoyCon Button is pressed for the first time, then
            //the calibration in Exp Manager will be set.
            JoyconController.ButtonPressed = true;
            calibrationGO.SetActive(false);
            mainMenuGO.SetActive(true);
        });
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

}
