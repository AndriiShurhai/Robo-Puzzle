using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolSlotUI : MonoBehaviour, 
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private ToolDefinition toolDefinition;

    private Image _icon;

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    private void Start()
    {
        if (toolDefinition != null && toolDefinition.icon != null)
        {
            _icon.sprite = toolDefinition.icon;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (toolDefinition == null) return;
        Debug.Log($"[ToolSlotUI] Begin drag of {toolDefinition.displayName}");
        ToolPlacementSystem.Instance?.BeginPlacement(toolDefinition);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ToolPlacementSystem.Instance?.EndPlacement();
    }
}