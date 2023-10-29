using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite unselected;
    public Sprite selected;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.image.sprite = unselected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        button.image.sprite = selected;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button.image.sprite = unselected;
    }

    public void ResetSprite()
    {
        button.image.sprite = unselected;
    }
}
