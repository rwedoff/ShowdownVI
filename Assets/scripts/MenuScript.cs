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
            SceneManager.LoadSceneAsync("SinglePlayer", LoadSceneMode.Single);
        });

        naiveButton.onClick.AddListener(() => {
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
