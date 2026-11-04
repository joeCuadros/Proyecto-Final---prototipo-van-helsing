
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CivilFearBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Civilian civilian;
    [SerializeField] private Image fearFill;
    [SerializeField] private TextMeshProUGUI percentageText;

    [Header("Colores")]
    [SerializeField] private Color calmColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color panicColor = Color.red;

    private void Awake()
    {
        if (civilian == null)
            civilian = GetComponentInParent<Civilian>();
    }

    private void Update()
    {
        if (civilian == null || fearFill == null)
            return;

        float fear = Mathf.Clamp(civilian.fearLevel, 0f, 100f);

        // Actualiza el relleno
        fearFill.fillAmount = fear / 100f;

        // Cambia el color según el miedo
        if (fear < 40f)
            fearFill.color = calmColor;
        else if (fear < 60f)
            fearFill.color = warningColor;
        else
            fearFill.color = panicColor;

        // Muestra el porcentaje centrado
        if (percentageText != null)
        {
            percentageText.text =
                Mathf.RoundToInt(fear).ToString() + "%";
        }
    }
}
