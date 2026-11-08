using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip compartido")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Texto de este elemento")]
    [TextArea]
    [SerializeField] private string message;

    private bool hovering;

    private void Update()
    {
        if (hovering)
        {
            tooltipPanel.transform.position =
                Input.mousePosition + new Vector3(15, 15, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipText.text = message;
        tooltipPanel.SetActive(true);
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        tooltipPanel.SetActive(false);
    }
}