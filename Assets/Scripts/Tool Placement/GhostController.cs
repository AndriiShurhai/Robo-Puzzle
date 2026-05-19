using UnityEngine;
using System.Collections;

public class GhostController : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("Semi-tranparent material applied to all renders. Need _BaseColor")]
    [SerializeField] private Material ghostMaterial;

    [Header("Colors")]
    [SerializeField] private Color validColor = new Color(0.2f, 1f, 0.2f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.5f);

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private GameObject _ghostInstance;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;

    public void Show(ToolDefinition tool)
    {
        Hide();

        _ghostInstance = Instantiate(tool.GetGhostPrefab(), transform);

        _renderers = _ghostInstance.GetComponentsInChildren<Renderer>(includeInactive: true);

        _propBlock = new MaterialPropertyBlock();

        if (ghostMaterial != null)
        {
            foreach (Renderer r in _renderers)
            {
                int count = r.sharedMaterials.Length;
                Material[] mats = new Material[count];
                for (int i = 0; i < count; i++) mats[i] = ghostMaterial;
                r.materials = mats;
            }
        }

        foreach (MonoBehaviour mb in _ghostInstance.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        {
            mb.enabled = false;
        }
        foreach (Collider c in _ghostInstance.GetComponentsInChildren<Collider>(includeInactive: true))
        {
            c.enabled = false;
        }

        gameObject.SetActive(true);
        SetValid(false);
    }

    public void SetPose(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
    public void SetValid(bool valid)
    {
        if (_renderers == null) return;

        Color color = valid ? validColor : invalidColor;

        foreach (Renderer r in _renderers)
        {
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorID, color);
            r.SetPropertyBlock(_propBlock);
        }
    }
    public void Hide()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }

        _renderers = null;
        gameObject.SetActive(false);
    }
}