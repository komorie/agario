using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    TMP_Text text;

    private void OnDisable()
    {
        text.color = Color.white;
    }

/*    //클릭 시 소리
    public void OnPointerDown(PointerEventData eventData)
    {
        if(gameObject.TryGetComponent(out Button button))
        {
            if(button.interactable)
            {
                SoundManager.Instance.Play("Sounds/ClickEffect");
            }
        }
    }*/

    //마우스 들어올 시 회색으로
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.TryGetComponent(out Button button))
        {
            if (button.interactable)
            {
                text.color = Color.gray;
            }
        }
    }

    //마우스 나갈 시 다시 흰색으로
    public void OnPointerExit(PointerEventData eventData)
    {
        if (gameObject.TryGetComponent(out Button button))
        {
            if (button.interactable)
            {
                text.color = Color.white;
            }
        }
    }
}
