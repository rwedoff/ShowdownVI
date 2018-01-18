using UnityEngine;
using UnityEngine.UI;

public class TutorialSounds : MonoBehaviour {
    public Button soundButton1;
    public Button soundButton2;
    public Button soundButton3;
    
    // Use this for initialization
    void Start()
    {
        AudioSource [] aSources = transform.GetComponents<AudioSource>();
        soundButton1.onClick.AddListener(() => { aSources[0].Play(); });
        soundButton2.onClick.AddListener(() => { aSources[1].Play(); });
        soundButton3.onClick.AddListener(() => { aSources[2].Play(); });
    }
}
