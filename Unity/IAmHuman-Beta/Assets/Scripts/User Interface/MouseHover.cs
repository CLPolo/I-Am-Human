using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite unselected;
    public Sprite selected;
    public bool disableParentOnClick = false;
    private AudioClip hoverSound;
    private AudioClip selectSound;
    private AudioSource audioSource;
    private const string path = "Sounds/SoundEffects/Entity/Interactable/Flashlight/";
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        if (unselected != null)
        {
            button.image.sprite = unselected;
        }
        hoverSound = Resources.Load<AudioClip>(path + "switch-flick-0");
        selectSound = Resources.Load<AudioClip>(path + "switch-flick-1");
        if ((audioSource = GetComponent<AudioSource>()) == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            AudioMixer mixer = Resources.Load("Master") as AudioMixer;
            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master").First();
        }
        button.onClick.AddListener(delegate {
            audioSource.PlayOneShot(selectSound, 0.5f);
            if (unselected != null)
            {
                button.image.sprite = unselected;
            }
            StartCoroutine(FinishClick());
        });
    }

    private IEnumerator FinishClick()
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);
        if (disableParentOnClick)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selected != null)
        {
            button.image.sprite = selected;
        }
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (unselected != null)
        {
            button.image.sprite = unselected;
        }
    }
}
