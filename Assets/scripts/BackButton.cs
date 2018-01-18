using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour {
    public Button backButton;
	// Use this for initialization
	void Start () {
        backButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single); });
    }
}
