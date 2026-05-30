using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolSlotUI : MonoBehaviour, 
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountText;

    private ToolDefinition _toolDefinition;
    private int _currentAmount;

    private void Awake()
    {
        _icon = GetComponent<Image>();
    }

    public void Setup(ToolDefinition tool, int amount)
    {
        _toolDefinition = tool;
        _icon.sprite = tool.icon;
        UpdateAmount(amount);
    }

    public void UpdateAmount(int amount)
    {
        _currentAmount = amount;
        _amountText.text = amount.ToString();

        // Dim the icon if we are out of stock
        _icon.color = _currentAmount > 0 ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_toolDefinition == null || _currentAmount <= 0) return;
        Debug.Log($"[ToolSlotUI] Begin drag of {_toolDefinition.displayName}");
        ToolPlacementSystem.Instance?.BeginPlacement(_toolDefinition);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ToolPlacementSystem.Instance?.EndPlacement();
    }
}