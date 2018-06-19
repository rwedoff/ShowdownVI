using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that is attached to all UI that should be accessible
/// </summary>
public class InputReader : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
    private AudioSource hoverAudioSource;

    public void Start()
    {
        var auds = GetComponents<AudioSource>();
        hoverAudioSource = auds[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (MenuScript.hoverTempAudioSource != null 
            && MenuScript.hoverTempAudioSource.isPlaying)
        {
            MenuScript.hoverTempAudioSource.Stop();
        }
        MenuScript.clickAudioSource.Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverAudioSource != null)
        {
            if (MenuScript.hoverTempAudioSource != null 
                && MenuScript.hoverTempAudioSource.isPlaying)
            {
                MenuScript.hoverTempAudioSource.Stop();
            }
            MenuScript.hoverTempAudioSource = hoverAudioSource;
            MenuScript.hoverTempAudioSource.Play();
        }
    }
}
