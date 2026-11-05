using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class EnemySignalVisual : MonoBehaviour
{
    public enum VisualAwarenessState
    {
        Normal,
        Alert,
        Chase,
        Disabled
    }

    [Header("Referencias")]
    [SerializeField] private AIBase ai;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("Direccion de mirada")]
    [SerializeField] private bool useManualFacing = false;
    [SerializeField] private Vector2 manualFacing = Vector2.down;

    [Header("Cono de vision")]
    [SerializeField] private float viewDistance = 2.2f;
    [SerializeField, Range(10f, 180f)] private float viewAngle = 60f;
    [SerializeField, Range(3, 40)] private int segments = 18;
    [SerializeField] private Vector3 coneOffset = Vector3.zero;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = -1;

    [Header("Colores")]
    [SerializeField] private Color normalColor = new Color(1f, 0.92f, 0f, 0.18f);
    [SerializeField] private Color alertColor = new Color(1f, 0.55f, 0f, 0.22f);
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.22f);
    [SerializeField] private Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.10f);

    [Header("Icono flotante")]
    [SerializeField] private bool showStateLabel = true;
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private float iconFontSize = 2.5f;

    [Header("Hooks futuros")]
    [SerializeField] private float forcedAlertUntil = -1f;

    private Transform coneRoot;
    private MeshFilter coneFilter;
    private MeshRenderer coneRenderer;
    private Material coneMaterial;

    private TextMeshPro label;

    private Vector2 lastLookDirection = Vector2.down;

    private float lastViewDistance;
    private float lastViewAngle;
    private int lastSegments;

    private void Awake()
    {
        if (ai == null)
            ai = GetComponent<AIBase>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (bodyRenderer == null)
            bodyRenderer = GetComponent<SpriteRenderer>();

        EnsureVisualChildren();
        RebuildConeMesh();
    }

    private void LateUpdate()
    {
        if (coneRoot == null || coneFilter == null || coneRenderer == null)
            return;

        if (viewDistance != lastViewDistance ||
            viewAngle != lastViewAngle ||
            segments != lastSegments)
        {
            RebuildConeMesh();
        }

        Vector2 lookDirection = GetLookDirection();
        UpdateConeTransform(lookDirection);

        VisualAwarenessState state = ResolveVisualState();
        ApplyStateVisuals(state);
    }

    public void ForceAlert(float duration = 1.2f)
    {
        forcedAlertUntil = Time.time + duration;
    }

    public void SetManualFacing(Vector2 direction)
    {
        useManualFacing = true;

        if (direction != Vector2.zero)
            manualFacing = direction.normalized;
    }

    public void ClearManualFacing()
    {
        useManualFacing = false;
    }

    public void SetViewShape(float distance, float angle)
    {
        viewDistance = distance;
        viewAngle = angle;
        RebuildConeMesh();
    }

    private void EnsureVisualChildren()
    {
        // CONO
        Transform coneTransform = transform.Find("VisionCone");

        if (coneTransform == null)
        {
            GameObject coneObject = new GameObject("VisionCone");
            coneObject.transform.SetParent(transform);
            coneObject.transform.localPosition = coneOffset;
            coneObject.transform.localRotation = Quaternion.identity;
            coneTransform = coneObject.transform;
        }

        coneRoot = coneTransform;

        coneFilter = coneRoot.GetComponent<MeshFilter>();
        if (coneFilter == null)
            coneFilter = coneRoot.gameObject.AddComponent<MeshFilter>();

        coneRenderer = coneRoot.GetComponent<MeshRenderer>();
        if (coneRenderer == null)
            coneRenderer = coneRoot.gameObject.AddComponent<MeshRenderer>();

        if (coneMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            coneMaterial = new Material(shader);
            coneMaterial.color = normalColor;
        }

        coneRenderer.material = coneMaterial;
        coneRenderer.sortingLayerName = sortingLayerName;
        coneRenderer.sortingOrder = sortingOrder;

        // ICONO / LABEL
        Transform labelTransform = transform.Find("StateLabel");

        if (labelTransform == null)
        {
            GameObject labelObject = new GameObject("StateLabel");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = iconOffset;
            labelObject.transform.localRotation = Quaternion.identity;
            labelTransform = labelObject.transform;
        }

        label = labelTransform.GetComponent<TextMeshPro>();

        if (label == null)
            label = labelTransform.gameObject.AddComponent<TextMeshPro>();

        label.text = "";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = iconFontSize;
        label.color = Color.white;
    }

    private void RebuildConeMesh()
    {
        if (coneFilter == null)
            return;

        Mesh mesh = new Mesh();
        mesh.name = "EnemyVisionCone";

        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];
        Color[] colors = new Color[vertexCount];

        vertices[0] = Vector3.zero;
        colors[0] = Color.white;

        float halfAngle = viewAngle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = -halfAngle + (viewAngle * t);

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * Vector2.up;

            vertices[i + 1] = dir * viewDistance;
            colors[i + 1] = Color.white;
        }

        int triangleIndex = 0;

        for (int i = 1; i <= segments; i++)
        {
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = i;
            triangles[triangleIndex++] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        coneFilter.sharedMesh = mesh;

        lastViewDistance = viewDistance;
        lastViewAngle = viewAngle;
        lastSegments = segments;
    }

    private Vector2 GetLookDirection()
    {
        if (useManualFacing && manualFacing != Vector2.zero)
        {
            lastLookDirection = manualFacing.normalized;
            return lastLookDirection;
        }

        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            lastLookDirection = rb.linearVelocity.normalized;
            return lastLookDirection;
        }

        return lastLookDirection;
    }

    private void UpdateConeTransform(Vector2 lookDirection)
    {
        coneRoot.localPosition = coneOffset;

        float angle =
            Mathf.Atan2(lookDirection.y, lookDirection.x) *
            Mathf.Rad2Deg - 90f;

        coneRoot.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private VisualAwarenessState ResolveVisualState()
    {
        if (Time.time < forcedAlertUntil)
            return VisualAwarenessState.Alert;

        if (ai == null)
            return VisualAwarenessState.Normal;

        string stateName = ai.currentState.ToString();

        if (stateName == "Seek" ||
            stateName == "Attack" ||
            stateName == "Cooldown")
        {
            return VisualAwarenessState.Chase;
        }

        if (stateName == "Alert")
        {
            return VisualAwarenessState.Alert;
        }

        if (stateName == "Stun" ||
            stateName == "Die")
        {
            return VisualAwarenessState.Disabled;
        }

        return VisualAwarenessState.Normal;
    }

    private void ApplyStateVisuals(VisualAwarenessState state)
    {
        if (coneMaterial == null)
            return;

        switch (state)
        {
            case VisualAwarenessState.Normal:
                coneMaterial.color = normalColor;
                SetLabel("");
                break;

            case VisualAwarenessState.Alert:
                coneMaterial.color = alertColor;
                SetLabel("?");
                break;

            case VisualAwarenessState.Chase:
                coneMaterial.color = chaseColor;
                SetLabel("!");
                break;

            case VisualAwarenessState.Disabled:
                coneMaterial.color = disabledColor;
                SetLabel("");
                break;
        }
    }

    private void SetLabel(string text)
    {
        if (label == null)
            return;

        label.transform.localPosition = iconOffset;

        if (!showStateLabel)
        {
            label.text = "";
            return;
        }

        label.text = text;
    }

    private void OnDestroy()
    {
        if (coneMaterial != null)
            Destroy(coneMaterial);
    }
}