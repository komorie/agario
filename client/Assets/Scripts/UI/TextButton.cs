using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : MonoBehaviour, IPointerUpHandler
{

    public void OnPointerUp(PointerEventData eventData)
    {
        if (gameObject.TryGetComponent(out Button button))
        {
            if (button.interactable)
            {
                eventData.selectedObject = null;
            }
        }
    }
}
