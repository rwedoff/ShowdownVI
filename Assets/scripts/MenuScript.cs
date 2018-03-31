using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    public Button expButton;
    public Button freeButton;
    public Button naiveButton;
    public Canvas menuCanvas;
    public Text expMenuText;

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
    }

}
