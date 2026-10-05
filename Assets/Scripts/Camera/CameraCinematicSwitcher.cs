using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[System.Serializable]
public class CameraTransitionEntry
{
    public string name;

    [Tooltip("La caméra virtuelle principale (laisser vide si vous utilisez un ClearShot)")]
    public CinemachineCamera vcam;

    [Tooltip("Le composant ClearShot si cette étape utilise une caméra ClearShot")]
    public CinemachineClearShot clearShot;

    [Tooltip("Durée pendant laquelle cette caméra reste active AVANT de passer à la suivante")]
    public float displayDuration = 3f;

    [Header("Transition vers cette caméra")]
    public CinemachineBlendDefinition.Styles blendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    public float blendDuration = 0.6f;
}

public class CameraCinematicSwitcher : MonoBehaviour
{
    [Tooltip("Séquence de la cinématique de début")]
    public List<CameraTransitionEntry> entries = new List<CameraTransitionEntry>();

    [Tooltip("Caméra de jeu principale du joueur (ex: Dolly Cam) vers laquelle revenir à la fin")]
    public CinemachineCamera playerDollyCamera;

    [Tooltip("Style de blend vers la caméra du joueur à la fin de la cinématique")]
    public CinemachineBlendDefinition.Styles finalBlendStyle = CinemachineBlendDefinition.Styles.EaseInOut;
    public float finalBlendDuration = 1.0f;

    [Tooltip("Lancer automatiquement la cinématique au Start")]
    public bool playOnStart = true;

    [Header("Priorités")]
    public int activePriority = 100;
    public int inactivePriority = 0;

    private CinemachineBrain brain;
    private CinemachineBlendDefinition originalDefaultBlend;
    private Coroutine runningTransition;

    void Awake()
    {
        if (Camera.main != null)
            brain = Camera.main.GetComponent<CinemachineBrain>();

        if (brain == null)
            brain = FindAnyObjectByType<CinemachineBrain>();

        if (brain != null)
            originalDefaultBlend = brain.DefaultBlend;
    }

    void Start()
    {
        if (playOnStart)
        {
            StartCinematic();
        }
    }

    public void StartCinematic()
    {
        StartCoroutine(PlayCinematicSequence());
    }

    /// <summary>
    /// Coroutine qui joue toute la séquence cinématique puis redonne la main à la Dolly Cam du joueur
    /// </summary>
    private IEnumerator PlayCinematicSequence()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // 1. Déclencher la transition vers la caméra/ClearShot
            if (entry.clearShot != null)
            {
                SwitchToClearShot(entry.clearShot, entry.blendStyle, entry.blendDuration);
            }
            else if (entry.vcam != null)
            {
                SwitchTo(entry.vcam, entry.blendStyle, entry.blendDuration);
            }

            // 2. Attendre pendant la durée de la transition + le temps d'affichage de la caméra
            yield return new WaitForSeconds(entry.blendDuration + entry.displayDuration);
        }

        // 3. Revenir à la caméra du joueur (Dolly Cam)
        if (playerDollyCamera != null)
        {
            SwitchTo(playerDollyCamera, finalBlendStyle, finalBlendDuration);
        }
    }

    public void SwitchTo(CinemachineCamera target, CinemachineBlendDefinition.Styles style, float duration)
    {
        if (target == null) return;
        EnsureBrain();

        var blend = new CinemachineBlendDefinition(style, duration);
        if (runningTransition != null)
            StopCoroutine(runningTransition);

        runningTransition = StartCoroutine(DoTransition(target.gameObject, blend));
    }

    public void SwitchToClearShot(CinemachineClearShot target, CinemachineBlendDefinition.Styles style, float duration)
    {
        if (target == null) return;
        EnsureBrain();

        var blend = new CinemachineBlendDefinition(style, duration);
        if (runningTransition != null)
            StopCoroutine(runningTransition);

        runningTransition = StartCoroutine(DoTransition(target.gameObject, blend));
    }

    private IEnumerator DoTransition(GameObject targetObj, CinemachineBlendDefinition blend)
    {
        var prev = brain.DefaultBlend;
        brain.DefaultBlend = blend;

        SetPriorities(targetObj);

        if (blend.Time > 0f)
            yield return new WaitForSeconds(blend.Time);
        else
            yield return null;

        brain.DefaultBlend = prev;
        runningTransition = null;
    }

    private void SetPriorities(GameObject activeObj)
    {
        // Réinitialiser les priorités des éléments de la liste
        foreach (var e in entries)
        {
            if (e.vcam != null)
                e.vcam.Priority = (e.vcam.gameObject == activeObj) ? activePriority : inactivePriority;

            if (e.clearShot != null)
                e.clearShot.Priority = (e.clearShot.gameObject == activeObj) ? activePriority : inactivePriority;
        }

        // Priorité de la caméra joueur
        if (playerDollyCamera != null)
        {
            playerDollyCamera.Priority = (playerDollyCamera.gameObject == activeObj) ? activePriority : inactivePriority;
        }
    }

    private void EnsureBrain()
    {
        if (brain == null)
        {
            brain = FindAnyObjectByType<CinemachineBrain>();
            if (brain != null)
                originalDefaultBlend = brain.DefaultBlend;
        }
    }

    public void RestoreDefaultBlend()
    {
        if (brain != null)
            brain.DefaultBlend = originalDefaultBlend;
    }
}