using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    public Button tutorialButton;
    public Button expButton;
    public Button freeButton;

	// Use this for initialization
	void Start () {
        tutorialButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync("tutorialMenu", LoadSceneMode.Single); });
        expButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync("ExpMode", LoadSceneMode.Single); });
        freeButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync("SinglePlayer", LoadSceneMode.Single); });
	}

}
