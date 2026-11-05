
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CivilRescueManager : MonoBehaviour
{
    public static CivilRescueManager Instance { get; private set; }

    [Header("Objetivo de rescate")]
    [Range(0f, 1f)]
    public float requiredRescueRatio = 0.70f;

    [Header("Interfaz")]
    public TextMeshProUGUI rescuedText;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI lostText;

    private readonly HashSet<CivilRescueMember> registered =
        new HashSet<CivilRescueMember>();

    private readonly HashSet<CivilRescueMember> rescued =
        new HashSet<CivilRescueMember>();

    private readonly HashSet<CivilRescueMember> lost =
        new HashSet<CivilRescueMember>();

    private int totalCivilians;
    private int requiredCivilians;
    private bool initialized;
    private bool finished;

    public int Total => totalCivilians;
    public int Rescued => rescued.Count;
    public int Lost => lost.Count;
    public int Required => requiredCivilians;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Hay dos CivilRescueManager en la escena.");
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Registrar los civiles colocados en la escena.
        CivilRescueMember[] members =
            FindObjectsByType<CivilRescueMember>(
                FindObjectsSortMode.None
            );

        foreach (CivilRescueMember member in members)
            RegisterCivil(member);

        totalCivilians = registered.Count;

        requiredCivilians = Mathf.CeilToInt(
            totalCivilians * requiredRescueRatio
        );

        initialized = true;

        RefreshUI();

        Debug.Log(
            $"Civiles: {totalCivilians}. " +
            $"Rescates necesarios: {requiredCivilians}"
        );

        if (totalCivilians == 0)
            Debug.LogWarning(
                "No se encontraron civiles con CivilRescueMember."
            );
    }

    public void RegisterCivil(CivilRescueMember member)
    {
        if (member == null || initialized)
            return;

        registered.Add(member);
    }

    public void RescueCivil(CivilRescueMember member)
    {
        if (!initialized || finished || member == null)
            return;

        if (!registered.Contains(member))
            return;

        if (lost.Contains(member))
            return;

        if (!rescued.Add(member))
            return;

        Debug.Log($"Civil rescatado: {member.name}");

        RefreshUI();
        CheckObjective();
    }

    public void CivilLost(CivilRescueMember member)
    {
        if (!initialized || finished || member == null)
            return;

        if (!registered.Contains(member))
            return;

        if (rescued.Contains(member))
            return;

        if (!lost.Add(member))
            return;

        Debug.Log($"Civil perdido: {member.name}");

        RefreshUI();
        CheckObjective();
    }

    private void CheckObjective()
    {
        if (finished || totalCivilians == 0)
            return;

        if (rescued.Count >= requiredCivilians)
        {
            finished = true;

            Debug.Log("¡Objetivo de rescate cumplido!");

            if (SystemManager.Instance != null)
                SystemManager.Instance.Victory();

            return;
        }

        int civiliansAvailable =
            totalCivilians - lost.Count;

        if (civiliansAvailable < requiredCivilians)
        {
            finished = true;

            Debug.Log("Ya no es posible rescatar al 70 %.");

            if (SystemManager.Instance != null)
                SystemManager.Instance.GameOver();
        }
    }

    private void RefreshUI()
    {
        if (rescuedText != null)
        {
            rescuedText.text =
                $"Rescatados: {rescued.Count}/{totalCivilians}";
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                $"Objetivo: {requiredCivilians} civiles";
        }

        if (lostText != null)
        {
            lostText.text =
                $"Perdidos: {lost.Count}";
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
