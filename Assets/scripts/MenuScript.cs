using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    public Button expButton;
    public Button freeButton;
    public Button naiveButton;
    public Canvas menuCanvas;
    public Text expMenuText;
    public InputField partInputField;
    public Button startRightHand;
    public Button startLeftHand;
    public Canvas startMenuCanvas;
    public InputField expInputField;
    public GameObject bodySourceViewObj;

	// Use this for initialization
	void Start () {
        expButton.onClick.AddListener(() => {
            ExperimentLog.Log("Pressed Exp Mode", "Menu");
            ExpManager.NaiveMode = false;
            menuCanvas.enabled = false;
            expMenuText.text = "Our Ball Exp";
            Time.timeScale = 1;
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

        naiveButton.onClick.AddListener(() => {
            ExperimentLog.Log("Pressed Naive Mode", "Menu");
            ExpManager.NaiveMode = true;
            menuCanvas.enabled = false;
            Time.timeScale = 1;
            expMenuText.text = "Naive Ball Exp";
            if (!SceneManager.GetActiveScene().name.Equals("Master"))
            {
                SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
            }
        });

        startRightHand.onClick.AddListener(() =>
        {
            if (!partInputField.text.Equals(""))
            {
                ExperimentLog.Log("Clicked [Start Right Hand]", tag: "pre-menu");
                StartMainMenu(false);
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
                StartMainMenu(true);
            }
            else
            {
                ExperimentLog.Log("Missing Participant ID", tag: "pre-menu");
                Debug.LogWarning("Missing Participant ID");
            }
        });
    }

    /// <summary>
    /// Sets Lefty mode, fills in the participant ID in Exp Panel and hides the Pre-menu
    /// </summary>
    /// <param name="isLefty"></param>
    private void StartMainMenu(bool isLefty)
    {
        expInputField.text = partInputField.text;
        startMenuCanvas.enabled = false;
        var script = bodySourceViewObj.GetComponent<BodySourceView>();
        script.SetLeftyToggle(isLefty);
    }

}
